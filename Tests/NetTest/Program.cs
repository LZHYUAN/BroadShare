using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace NetTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var CC = new NetClient(12345);

            CC.ClientReceived += CC_ClientReceived;

            while (true)
            {
                Console.ReadKey();
                CC.ClientRequest();
            }


            Task.Delay(-1).Wait();

        }

        private static void CC_ClientReceived(IPAddress arg1, PhysicalAddress arg2, string arg3)
        {
            Console.WriteLine(arg1 + " , " + arg2 + " , " + arg3);
        }
    }
}