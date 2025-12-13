using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MidChat.Server // Espace de noms
{
    internal class ChatServer : System.Object
    {
        public void Run() //Methode
        {
            const int port = 5000;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Serveur démarré sur le port {port}");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connecté");

                var thread = new Thread(HandleClient);
                thread.Start(client);
            }
        }

        private static void HandleClient(object obj)
        {
            if (obj is TcpClient tcpClient)
            {
                using (tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    byte[] buffer = new byte[1024];

                    while (true)
                    {
                       int bytesRead = stream.Read(buffer, 0, buffer.Length);
                       if (bytesRead == 0)
                           break; //Déconnexion du client

                       string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                       Console.WriteLine($"Reçu: {message}");
                    }
                }
            }
        }
    }
}
