using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Clients.Windows.Main
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World!");

            var address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            var endPoint = new IPEndPoint(address, 54248);
            var listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Starting to bind...");

            listener.Bind(endPoint);
            listener.Listen(10);

            Console.WriteLine("Bound port");

            while (true)
            {
                Console.WriteLine("Accepting connection...");
                var client = await listener.AcceptAsync();
                Console.WriteLine("Got connection");

                client.Send(Encoding.ASCII.GetBytes("Hello World!"));

                client.Close();
            }
        }
    }
}
