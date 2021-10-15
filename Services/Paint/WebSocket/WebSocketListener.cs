using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Paint.WebSocketPaint
{

    public class WebSocketListener : IWebSocketListener
    {
        public async Task Listen(WebSocket socket, Action<WebSocketReceiveResult, string> action)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                action(result, Encoding.UTF8.GetString(buffer, 0, result.Count));

                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }
    }
}