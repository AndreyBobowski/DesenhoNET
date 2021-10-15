using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Services.Paint.WebSocketPaint
{
    public interface IWebSocketPaintClient : IReciver, IDisposable
    {
        Task Run(string address, WebSocket socket);
    }
}