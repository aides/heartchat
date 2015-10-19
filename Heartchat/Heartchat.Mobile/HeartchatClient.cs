using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Heartchat.Client
{
    public class DrawEventHandlerArgs : EventArgs
    {
        public Windows.Foundation.Point[] Points { get; private set; }

        public DrawEventHandlerArgs(Windows.Foundation.Point[] points)
	    {
            this.Points = points;
	    }
    }

    public class VibrateEventHandlerArgs : EventArgs
    {
        public double Duration { get; private set; }

        public VibrateEventHandlerArgs(double duration)
        {
            this.Duration = duration;
        }
    }

    public class HeartchatClient
    {
        private HubConnection connection;
        private IHubProxy hub;
        private string id;

        public event EventHandler<DrawEventHandlerArgs> OnDraw;
        public event EventHandler OnClear;
        public event EventHandler<VibrateEventHandlerArgs> OnVibrate;

        public HeartchatClient(string hostName, int portNumber)
        {
            this.connection = new HubConnection(string.Format("http://{0}:{1}/", hostName, portNumber));
            this.hub = connection.CreateHubProxy("HeartChatHub");

            this.hub.On("Draw", (Windows.Foundation.Point[] points) =>
                {
                    if (this.OnDraw != null)
                    {
                        this.OnDraw(this, new DrawEventHandlerArgs(points));
                    }
                });

            this.hub.On("Clear", () =>
            {
                if (this.OnClear != null)
                {
                    this.OnClear(this, new EventArgs());
                }
            });

            this.hub.On("Vibrate", (double duration) =>
            {
                if (this.OnVibrate != null)
                {
                    this.OnVibrate(this, new VibrateEventHandlerArgs(duration));
                }
            });
        }

        public async Task Connect()
        {
            await this.connection.Start(new LongPollingTransport());
            this.id = await this.hub.Invoke<string>("Join", "Nokia 1520");
        }

        public string GetId()
        {
            return this.id;
        }

        public async void SendDraw(List<Windows.Foundation.Point> points)
        {
            List<Point> newPoints = points.Select(p => new Point{ X = p.X, Y = p.Y }).ToList();

            this.hub.Invoke("Draw", new object[] { this.id, newPoints });
        }

        internal void Clear()
        {
            this.hub.Invoke("Clear", new object[] { this.id });
        }

        public void Vibrate(double duration)
        {
            this.hub.Invoke("Vibrate", new object[] { this.id, duration });
        }
    }

    public class Point
    {
        public Point()
        {

        }
        public double X { get; set; }

        public double Y { get; set; }
    }
}
