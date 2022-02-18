using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_MCLauncher
{
    internal class Server
    {
        private Socket socket;

        private enum Modpacks
        {
            BlockyCrafters,
            ProjectCenturos
        }


        // Start point from server.
        public void Start()
        {
            while (Connect() == null)
                Thread.Sleep(3000);

            Listen();
        }

        // Start listen for connections.
        private Socket Connect()
        {
            try
            {
                Console.WriteLine("Starting server..");
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 20);
                socket.Bind(endPoint);
                Console.WriteLine("Server started.");

                return socket;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to start server.");
                return null;
            }
        }

        // Listen for connections.
        private void Listen()
        {
            while (true)
            {
                Thread.Sleep(1000);
                socket.Listen(100);
                Task.Run(new Action(HandleTcpClient));
            }
        }

        // Handle client request.
        private void HandleTcpClient()
        {
            Socket client = socket.Accept();
            Console.WriteLine("Connected with {0}", client.RemoteEndPoint);

            byte[] buffer = new byte[client.SendBufferSize];
            int length = client.Receive(buffer);
            byte[] bytes = new byte[length];

            for (int index = 0; index < length; index++)
                bytes[index] = buffer[index];

            string data = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(string.Format("Command \"{0}\" from {1}", data, client.RemoteEndPoint));

            GetCommand(data, client);
        }

        // Redirect to requested command.
        private void GetCommand(string data, Socket client)
        {
            string[] command = data.Split(' ');
            switch (command[0])
            {
                case "VER":
                    GetVersion(client);
                    break;

                case "PAT":
                    GetPatch(client);
                    break;

                case "NEW":
                    ModInstall((Modpacks)int.Parse(command[1]), client);
                    break;

                case "UPD":
                    ModUpdate((Modpacks)int.Parse(command[1]), client);
                    break;

                case "C_VER":
                    GetLauncherVersion(client);
                    break;

                case "C_UPD":
                    LauncherUpdate(client);
                    break;


                default:
                    break;
            }
        }

        #region Commands
        private void GetVersion(Socket client)
        {
            string path = "Modpack/Version.txt";

            byte[] bytes = Encoding.ASCII.GetBytes(File.ReadAllText(path));
            client.Send(bytes);
            Console.WriteLine("Data sent!");
        }

        private void GetPatch(Socket client)
        {
            string path = "Modpack/Patch.txt";

            byte[] bytes = Encoding.ASCII.GetBytes(File.ReadAllText(path));
            client.Send(bytes);
            Console.WriteLine("Data sent!");

        }

        private void ModUpdate(Modpacks modpack, Socket client)
        {
            string path = $"Modpack/{modpack}_update.zip";

            Console.WriteLine("Sending {0} update to {1}", modpack, client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("{0} send!", modpack);

            client.Close();
        }

        private void ModInstall(Modpacks modpack, Socket client)
        {
            string path = $"Modpack/{modpack}.zip";

            Console.WriteLine("Sending {0} to {1}", modpack, client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("{0} send!", modpack);

            client.Close();
        }

        private void GetLauncherVersion(Socket client)
        {
            string path = "Launcher/Version.txt";

            byte[] bytes = Encoding.ASCII.GetBytes(File.ReadAllText(path));
            client.Send(bytes);
            Console.WriteLine("Data sent!");
        }

        private void LauncherUpdate(Socket client)
        {
            string path = $"Launcher/launcher.zip";

            Console.WriteLine("Sending launcher update to {0}", client.RemoteEndPoint);
            client.SendFile(path);
            Console.WriteLine("launcher send!");

            client.Close();
        }

        #endregion
    }
}
