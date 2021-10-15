using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Services.Paint.WebSocketPaint
{
    public interface IWebSocketListener
    {
        Task Listen(WebSocket socket, Action<WebSocketReceiveResult, string> action);
    }
}