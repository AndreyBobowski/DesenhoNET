using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Net;
using Microsoft.AspNetCore.Http;
using Services.Paint;
using System.Threading;
using Services.Paint.WebSocketPaint;

namespace NetCore.Teste.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private readonly IPaintServiceFactory _paintFactory;
        private readonly IWebSocketPaintServer _socketServer;
        private readonly IWebSocketPaintClient _socketClient;
        public WebSocketController(IPaintServiceFactory paintFactory, IWebSocketPaintServer socketServer, IWebSocketPaintClient socketClient)
        {
            _paintFactory = paintFactory;
            _socketServer = socketServer;
            _socketClient = socketClient;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await
                                   HttpContext.WebSockets.AcceptWebSocketAsync();
                await StartPaint(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
       
        private async Task StartPaint(HttpContext httpContext, WebSocket webSocket)
        {
            try
            {
                if (httpContext.Request.Query.ContainsKey("con"))
                    await _socketClient.Run(httpContext.Request.Query["con"], webSocket);
                else
                    await _socketServer.Run(httpContext.Request.Query["con"], webSocket);
            }
            catch (Exception e)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, e.Message, CancellationToken.None);
            }
        }
    }
}


