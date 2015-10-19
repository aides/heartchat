using System;
using Microsoft.Owin.Hosting;

namespace Heartchat.Server
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Console.WriteLine("App started...");
            using (WebApp.Start<Startup>("http://*:5555/"))
            {
                Console.WriteLine("Server running at http://10.0.9.153:5555/");
                Console.ReadLine();
            }
            Console.WriteLine("App finished...");
        }

        #endregion
    }
}