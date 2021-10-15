using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCore.Teste;

namespace Services.Paint.WebSocketPaint
{

    public class WebSocketPaintClient : IWebSocketPaintClient
    {
        private readonly IPaintServiceFactory _paintFactory;
        private readonly IWebSocketListener _socketListener;
        private IPaintClient _client;
        private string _address;
        private WebSocket _socket;

        public WebSocketPaintClient(IPaintServiceFactory paintFactory, IWebSocketListener socketListener)
        {
            _paintFactory = paintFactory;
            _socketListener = socketListener;
        }

        public async Task Run(string address, WebSocket socket)
        {
            _socket = socket;
            _address = address;
            _client = _paintFactory.CreateClient(address, this);
            SendGretings();
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
            if(_socket.State == WebSocketState.Open)
                 _socket.CloseAsync(status, description, cancellationToken).Wait();
            _client.Send("{\"action\":\"disconnect\"}");
        }

        private void SendGretings()
        {
            _client.Send("{\"action\":\"connect\"}");
        }
        private void OnListening(WebSocketReceiveResult result, string message)
        {
            _client.Send(message);
            if (result.CloseStatus.HasValue)
                EndWebSocket(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task Recive(string msg)
        {
            var messageAction = JsonSerializer.Deserialize<MessageAction>(msg);
            await _socket.SendAsync(stringToArrayFragment(msg), WebSocketMessageType.Text, true, CancellationToken.None);
            if (messageAction.action == "disconnect")
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Servidor Fechado", CancellationToken.None);
                return;
            }
        }

        private ArraySegment<byte> stringToArrayFragment(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            return new ArraySegment<byte>(msg, 0, msg.Length);
        }
        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}