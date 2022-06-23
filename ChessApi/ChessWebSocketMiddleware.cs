using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace ChessApi;

public class ChessWebSocketMiddleware
{
    public static ConcurrentDictionary<string, WebSocket> Sockets = new ConcurrentDictionary<string, WebSocket>();

    private readonly RequestDelegate _next;

    public ChessWebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }

        CancellationToken ct = context.RequestAborted;
        WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
        var socketId = context.Request.Query["id"];
        
        Sockets.TryAdd(socketId, currentSocket);

        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var str = await ReceiveStringAsync(currentSocket, ct);
            if (string.IsNullOrEmpty(str))
            {
                if (currentSocket.State != WebSocketState.Open)
                {
                    break;
                }

                continue;
            }

            var response = JsonConvert.DeserializeObject<WebSocketMessage>(str);

            foreach (var socket in Sockets)
            {
                if (socket.Value.State != WebSocketState.Open || socket.Key != response.ToUserId)
                {
                    continue;
                }

                await SendStringAsync(socket.Value, JsonConvert.SerializeObject(response), ct);
            }
        }

        WebSocket dummy;
        Sockets.TryRemove(socketId, out dummy);

        await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
        currentSocket.Dispose();
    }

    public static Task SendStringAsync(WebSocket socket, string data,
        CancellationToken ct = default(CancellationToken))
    {
        var buffer = Encoding.UTF8.GetBytes(data);
        var segment = new ArraySegment<byte>(buffer);
        return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
    }

    private static async Task<string> ReceiveStringAsync(WebSocket socket,
        CancellationToken ct = default(CancellationToken))
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        using (var ms = new MemoryStream())
        {
            WebSocketReceiveResult result;
            do
            {
                ct.ThrowIfCancellationRequested();

                result = await socket.ReceiveAsync(buffer, ct);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            if (result.MessageType != WebSocketMessageType.Text)
            {
                return null;
            }

            using (var reader = new StreamReader(ms, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}