using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net.WebSockets;
using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using server.Models;

namespace server.Core
{
    public class WebSocketConnection
    {

        private WebSocket websocket;

        private Environment environment;

        private System.Timers.Timer timer;

        public WebSocketConnection(WebSocket websocket, HttpContext context)
        {
            this.websocket = websocket;

            int width = Convert.ToInt32(context.Request.Query["width"]);
            int height = Convert.ToInt32(context.Request.Query["height"]);

            this.environment = new Environment(width, height);

            this.timer = new System.Timers.Timer();
            this.timer.Interval = 100;
            this.timer.Elapsed += Draw;
            this.timer.Enabled = true;

            this.environment.Start();
        }


        public async Task Watch()
        {
            await Receive(this.websocket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var request = JsonSerializer.Deserialize<RequestModel>(message);

                    Specie specie = Specie.Seaweed;

                    switch (request.animal)
                    {
                        case "SHARK": specie = Specie.Shark; break;
                        case "SEAL": specie = Specie.Seal; break;
                        case "FISH": specie = Specie.Fish; break;
                        case "SEAWEED": specie = Specie.Seaweed; break;
                    }

                    this.environment.AddAnimal(specie, request.calorias);

                    return;
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    this.environment.Destroy();
                    this.timer.Enabled = false;
                }
            });
        }


        private async void Draw(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (this.websocket.State == WebSocketState.Open)
            {
                var grid = this.environment.GetGrid();

                var json = JsonSerializer.Serialize(grid);

                await this.websocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }

    }
}