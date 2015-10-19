using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Heartchat.Server
{
    public class Point
    {
        public double X { get; set; }

        public double Y { get; set; }
    }
    public class HeartChatHub : Hub
    {
        public string Join(string name)
        {
            Console.WriteLine("{0} joined.", name);

            Guid guid = Guid.NewGuid();
            
            Console.WriteLine("Assigned guid {0} to {1}", guid, name);

            return guid.ToString();
        }

        public void Vibrate(string id, double duration)
        {
            Console.WriteLine("{0} vibrates for {1} secs.", id, duration);

            this.Clients.Others.Vibrate(duration);
        }

        public void Draw(string id, Point[] points)
        {
            Console.WriteLine("{0} draws {1} points.", id, points.Length);

            this.Clients.Others.Draw(points);
        }

        public void Clear(string id)
        {
            Console.WriteLine("{0} cleared.", id);
            this.Clients.Others.Clear();
        }
    }
}
