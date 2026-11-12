# C# deployment — Ubuntu 24.04 x86_64

Target: https://csharp.dimasfrmnsyh.my.id. Existing portfolio and Express sites are separate and must not be overwritten.

Two self-contained .NET 10 apps: StudentApi on loopback 5100, WebSocketApp on loopback 5101. No .NET SDK/runtime installation required; Linux native dependencies are still required. SQLite data is stored at /var/lib/assessment-students/Data/students.db, outside release files. These are public learning demos without authentication: use fictional student data only.

## 1. DNS

A record: csharp -> 149.41.196.223. Check `dig +short csharp.dimasfrmnsyh.my.id A`.

## 2. Prepare VPS as deploy

```bash
sudo mkdir -p /srv/csharp-assignment
sudo chown deploy:deploy /srv/csharp-assignment
sudo apt update
sudo apt install -y ca-certificates libc6 libgcc-s1 libgssapi-krb5-2 libicu74 libssl3t64 libstdc++6 zlib1g
```

## 3. Upload FROM MAC

```bash
scp /Users/dimasfrmnsyh/Documents/assessment/csharp-assignment/artifacts/csharp-release-linux-x64.tar.gz deploy@149.41.196.223:/srv/csharp-assignment/
```

## 4. Install ON VPS

```bash
cd /srv/csharp-assignment
tar -xzf csharp-release-linux-x64.tar.gz
chmod +x StudentApi/StudentApi WebSocketApp/WebSocketApp
sudo install -m 644 deployment/student-api.service /etc/systemd/system/student-api.service
sudo install -m 644 deployment/websocket-app.service /etc/systemd/system/websocket-app.service
sudo systemctl daemon-reload
sudo systemctl enable --now student-api websocket-app
sudo systemctl status student-api websocket-app --no-pager
curl -i http://127.0.0.1:5100/students
curl -I http://127.0.0.1:5101/
```

Both services should be active. Student list initially returns []. Do not expose 5100/5101 in firewall.

## 5. Nginx — first installation only

Do not overwrite this config after Certbot has added HTTPS settings.

```bash
sudo install -m 644 deployment/nginx-csharp /etc/nginx/sites-available/csharp
sudo ln -s /etc/nginx/sites-available/csharp /etc/nginx/sites-enabled/csharp
sudo nginx -t
```

Only after validation succeeds:

```bash
sudo systemctl reload nginx
sudo certbot --nginx -d csharp.dimasfrmnsyh.my.id
sudo certbot renew --dry-run
```

## 6. Demo and verification

Open https://csharp.dimasfrmnsyh.my.id, press Connect, Send, then Disconnect. Browser should use wss://csharp.dimasfrmnsyh.my.id/ws and echo the entered message.

REST examples (use returned ID instead of assuming 1):

```bash
curl -i https://csharp.dimasfrmnsyh.my.id/students
curl -i https://csharp.dimasfrmnsyh.my.id/students -H 'Content-Type: application/json' -d '{"name":"Demo Student","email":"demo@example.com","major":"Computer Science"}'
```

Use PUT and DELETE /students/<id> as documented in the application README. Create a fictional record, restart `student-api`, verify it remains, then delete that record.

## Logs and restart

```bash
sudo journalctl -u student-api -n 50 --no-pager
sudo journalctl -u websocket-app -n 50 --no-pager
sudo systemctl restart student-api websocket-app
```

## Backups and updates

For a simple consistent backup, stop student-api, copy the entire /var/lib/assessment-students/Data directory (including SQLite WAL sidecars if present) to a timestamped private backup, then restart it. Do not copy a live SQLite main file alone. Keep database backups private.

For updates, stop both C# services before replacing their binaries, retain the previous release for rollback, extract new binaries, then start services. Do not delete /var/lib/assessment-students or replace the Certbot-managed Nginx config. To roll back, restore previous binaries and restart services; database schema compatibility must be assessed for future schema changes.

## Verification limits

A Linux-target publish from Mac is not proof of execution on Linux. The final deployment is complete only after VPS service, HTTPS REST CRUD, persistence, and WSS echo checks pass. There is no login, authorization, per-user isolation, email verification, or application-level abuse control in these learning apps.
