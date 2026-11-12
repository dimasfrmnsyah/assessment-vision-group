using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("A WebSocket connection is required.");
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4096];
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseOutputAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    result.CloseStatusDescription, context.RequestAborted);
                break;
            }

            // Echo each fragment without changing the message type or boundaries.
            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                result.MessageType, result.EndOfMessage, context.RequestAborted);
        }
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
    catch (WebSocketException)
    {
        app.Logger.LogInformation("WebSocket client disconnected.");
    }
});

app.Run();
