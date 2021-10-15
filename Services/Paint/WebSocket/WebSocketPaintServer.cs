using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCore.Teste;

namespace Services.Paint.WebSocketPaint
{

    public class WebSocketPaintServer : IWebSocketPaintServer
    {
        private readonly IPaintServiceFactory _paintFactory;
        private readonly IWebSocketListener _socketListener;
        private IPaintServer _server;
        private string _address;
        private WebSocket _socket;

        public WebSocketPaintServer(IPaintServiceFactory paintFactory, IWebSocketListener socketListener)
        {
            _paintFactory = paintFactory;
            _socketListener = socketListener;
        }
        public async Task Run(string address, WebSocket socket)
        {
            _socket = socket;
            _address = address;
            _server = _paintFactory.CreateServer(this);
            await SendConnectAddress();
            try
            {
                await _socketListener.Listen(_socket, OnListening);
            }catch(Exception e)
            {
                EndWebSocket(WebSocketCloseStatus.InternalServerError, e.Message, CancellationToken.None);
            }finally
            {
                EndWebSocket(WebSocketCloseStatus.NormalClosure, "Server Encerrado", CancellationToken.None);
            }
        }
        private void EndWebSocket(WebSocketCloseStatus status, string description, CancellationToken cancellationToken)
        {
            if (_socket.State == WebSocketState.Open)
                _socket.CloseAsync(status, description, cancellationToken).Wait();
            _server.Send("{\"action\":\"disconnect\"}");
        }
        private void OnListening(WebSocketReceiveResult result, string message)
        {
            _server.Send(message);
            if (result.CloseStatus.HasValue)
                EndWebSocket(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        private async Task SendConnectAddress()
        {
            var name = _server.GetAddress();
            await _socket.SendAsync(stringToArrayFragment("{\"action\":\"serverid\", \"data\":\"" + name + "\"}"), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public async Task Recive(string msg)
        {
            var messageAction = JsonSerializer.Deserialize<MessageAction>(msg);
            await _socket.SendAsync(stringToArrayFragment(msg), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private ArraySegment<byte> stringToArrayFragment(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return new ArraySegment<byte>(msg, 0, msg.Length);
        }
        public void Dispose()
        {
            if (_server != null)
                _server.Dispose();
        }
    }
}