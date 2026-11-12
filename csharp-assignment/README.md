# C# Student CRUD and WebSocket Assignment

Two small ASP.NET Core applications written in C# with .NET 10:

- **StudentApi:** student CRUD backed by a local SQLite database.
- **WebSocketApp:** a WebSocket echo server with a small browser client.

## Requirements

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
Check your installation with `dotnet --version`. On this Mac, run
`source ~/.zprofile` first if the command is not found.

SQLite does not need a separate database server. The API uses the
`Microsoft.Data.Sqlite` NuGet package, which restores automatically on the first run.

## Run locally

Open a terminal in this directory:

```bash
cd /Users/dimasfrmnsyh/Documents/assessment/csharp-assignment
dotnet run --project StudentApi --urls http://localhost:5000
```

In a second terminal, open this directory and run:

```bash
dotnet run --project WebSocketApp --urls http://localhost:5001
```

Keep both terminals open. Press Ctrl+C to stop an app.
If an app is already running on one of these ports, stop that instance before starting another.

- API: http://localhost:5000/students
- WebSocket browser demo: http://localhost:5001
- WebSocket endpoint: ws://localhost:5001/ws
- API documentation: http://localhost:5001/docs/

The public documentation URL is https://csharp.dimasfrmnsyh.my.id/docs/.
It is served from `WebSocketApp/wwwroot/docs/index.html` through the existing
web proxy. Documentation examples target the public REST API.

In the browser demo, click **Connect**, enter a message, and click **Send**.
The server returns the same message. Click **Disconnect** to close the connection.

## Student endpoints

| Method | Path | Description | Success |
|---|---|---|---|
| GET | `/students` | List all students | 200 |
| GET | `/students/{id}` | Get one student | 200 |
| POST | `/students` | Create a student | 201 |
| PUT | `/students/{id}` | Replace student fields | 200 |
| DELETE | `/students/{id}` | Delete a student | 204 |

POST and PUT require `name`, `email`, and `major`. The server generates `id`.
Invalid input returns 400. A missing student returns 404 for valid requests.
PUT replaces all three editable fields; it is not a partial update.

### Create

```bash
curl -i http://localhost:5000/students \
  -H 'Content-Type: application/json' \
  -d '{"name":"Alice Johnson","email":"alice@example.com","major":"Computer Science"}'
```

Example response (your ID may differ):

```json
{"id":1,"name":"Alice Johnson","email":"alice@example.com","major":"Computer Science"}
```

Use the returned ID in the remaining examples.

### Read

```bash
curl http://localhost:5000/students
curl http://localhost:5000/students/1
```

### Update

```bash
curl -i -X PUT http://localhost:5000/students/1 \
  -H 'Content-Type: application/json' \
  -d '{"name":"Alice Smith","email":"alice.smith@example.com","major":"Mathematics"}'
```

### Delete

```bash
curl -i -X DELETE http://localhost:5000/students/1
```

A successful deletion returns 204 with no response body.

## SQLite database

The app creates `Data/students.db` under its content root on startup.
With the local run command above, this is `StudentApi/Data/students.db`.
The `Students` table contains `Id`, `Name`, `Email`, and `Major`.
Data survives application restarts. The database is ignored by Git.
The app opens real SQLite connections for all CRUD operations and uses SQL parameters.

On this Mac, inspect it from the assignment directory:

```bash
sqlite3 StudentApi/Data/students.db 'SELECT Id, Name, Email, Major FROM Students;'
```

To check persistence yourself, create a student, stop the API with Ctrl+C,
start it again with the same command, and fetch that student's ID.

## Code guide

- `StudentApi/Program.cs`: HTTP endpoints, student records, and input validation.
- `StudentApi/StudentStore.cs`: SQLite connections, schema creation, and SQL CRUD.
- `WebSocketApp/Program.cs`: WebSocket connection and echo handling.
- `WebSocketApp/wwwroot/index.html`: browser client.

An HTTP request reaches an endpoint, input is validated, the store executes SQL,
and ASP.NET Core returns the result as JSON. WebSockets keep a connection open so
messages can flow without creating a new HTTP request for each message.

## Linux deployment status

Deployment is intentionally deferred until local verification is approved.
No Linux server has been contacted. The server distribution and architecture still
need to be confirmed before preparing and testing its deployment commands.
The apps are learning demos without authentication.

## References

- [Microsoft.Data.Sqlite overview](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
- [SQL parameters](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/parameters)
- [.NET publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
