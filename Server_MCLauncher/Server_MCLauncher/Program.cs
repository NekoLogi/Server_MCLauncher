using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_MCLauncher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "File-Server for minecraft";

            Server server = new Server();
            server.Start();
        }
    }
}
