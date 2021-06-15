using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace WindowsClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            IPEndPoint endPoint = new IPEndPoint(address, 54248);
            Socket listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
