using log4net;
using log4net.Config;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MidChat.Server // Espace de noms
{
    internal class ChatServer : System.Object
    {
        private static List<TcpClient> TcpClients = new List<TcpClient>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatServer));

        public ChatServer()
        {
            //Pour gérer la couleur dans la console (obligatoire)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var logRepository = LogManager.GetRepository();
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config")); //Cherche le fichier de configuration log4net dans le répertoire où se trouve l'exécutable (tjs mettre Copy always sur le fichier log4net.config)

            Log.Info("ChatServer initialized.");
        }

        public void Run() //Methode
        {
            try
            {
                const int port = 5000;
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Log.Info($"Serveur démarré sur le port {port}");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Log.Warn("Client connecté");
                    TcpClients.Add(client);

                    var thread = new Thread(HandleClient);
                    thread.Start(client);
                }
            }
            catch (Exception e)
            {
                Log.Error("Erreur inattendue", e);
            }
        }

        private static void HandleClient(object obj)
        {
            if (obj is TcpClient tcpClient)
            {
                try
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

                            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Log.Debug($"Reçu: {message}");

                            foreach (var client in TcpClients)
                            {
                                if (client == tcpClient)
                                    continue; //Ne pas renvoyer au client émetteur

                                var clientStream = client.GetStream();
                                byte[] data = Encoding.ASCII.GetBytes(message);
                                clientStream.Write(data, 0, data.Length);
                                clientStream.Flush();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Erreur inattendue", e);
                    TcpClients.Remove(tcpClient);
                }
            }
        }
    }
}
