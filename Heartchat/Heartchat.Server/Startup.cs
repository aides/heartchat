using Microsoft.AspNet.SignalR;
using Owin;

namespace Heartchat.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}