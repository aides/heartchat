using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Heartchat.Server
{
    class HeartchatServer
    {
        private static int Port = 5555;

        public static ManualResetEvent syncEvent = new ManualResetEvent(false);
        public void Start()
        {
            Console.WriteLine("Heartchat server starting...");

            byte[] buffer = new byte[1024];

            var permission = new SocketPermission(NetworkAccess.Accept,
                   TransportType.Tcp, "", SocketPermission.AllPorts);

            IPHostEntry ipHost = Dns.GetHostEntry("");
            IPAddress ipAddr = ipHost.AddressList[3]; // WTF?
            var ipEndPoint = new IPEndPoint(ipAddr, Port); 

            var socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Console.WriteLine("Binding on {0}:{1}", ipEndPoint.Address, ipEndPoint.Port);
                socket.Bind(ipEndPoint);
                socket.Listen(10);

                while (true)
                {
                    syncEvent.Reset();

                    Console.WriteLine("Waiting for connection...");

                    socket.BeginAccept(new AsyncCallback(OnAcceptCallback), socket);

                    syncEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR!" + e.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private void OnAcceptCallback(IAsyncResult ar)
        {
            syncEvent.Set();

            Socket socket = (Socket) ar.AsyncState;
            Socket handler = socket.EndAccept(ar);

            ChatState state = new ChatState();
            state.Socket = handler;
            handler.BeginReceive(state.Buffer, 0, ChatState.BufferSize, 0, new AsyncCallback(OnRecieveCallback), state);
        }

        private void OnRecieveCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            ChatState state = (ChatState) ar.AsyncState;
            Socket handler = state.Socket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                content += Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
                Console.WriteLine("Recieved {0} bytes from server. Data: {1}", content.Length, content);

                Send(handler, content);
            }

        }

        private void Send(Socket handler, string content)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(content);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(OnSendCallback), handler);

        }

        private void OnSendCallback(IAsyncResult ar)
        {

            try
            {
                Socket handler = (Socket) ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! {0}", e.Message);
                throw;
            }
        }
    }
}
