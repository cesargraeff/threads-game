using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using server.Core;

namespace server.Middleware
{
    public class WebSocketServerMiddleware
    {
        public WebSocketServerMiddleware(RequestDelegate next)
        { }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var websocket = await context.WebSockets.AcceptWebSocketAsync();

                var connection = new WebSocketConnection(websocket, context);
                await connection.Watch();
            }
        }
    }
}