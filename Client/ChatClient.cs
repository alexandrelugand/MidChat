using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class ChatClient
    {
        public void Run()
        {
            const string serverIp = "127.0.0.1";
            const int serverPort = 5000;

            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(serverIp, serverPort);
                Console.WriteLine("Connecté au serveur de chat.");

                var stream = tcpClient.GetStream();

                while (true)
                { 
                    var msg = Console.ReadLine();

                    if (string.IsNullOrEmpty(msg))
                        continue;

                    if (string.Compare(msg, "exit", StringComparison.OrdinalIgnoreCase) == 0) //msg == "exit" ou msg == "EXIT"
                        break;

                    byte[] data = Encoding.ASCII.GetBytes(msg);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Serveur: {response}");
                }
            }
        }
    }
}
