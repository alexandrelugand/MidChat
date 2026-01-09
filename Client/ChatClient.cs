using log4net;
using log4net.Config;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class ChatClient
    {
        static readonly ILog log = LogManager.GetLogger(typeof(ChatClient));
        TcpClient client = new();
        NetworkStream? stream;
        bool running = true;

        public ChatClient()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Run()
        {
            try
            {
                XmlConfigurator.Configure(new FileInfo("log4net.config"));

                client.Connect("127.0.0.1", 5000);
                stream = client.GetStream();
                log.Info("Connecté");

                new Thread(Read).Start();

                while (running)
                {
                    var msg = Console.ReadLine();
                    if (msg == "exit") break;
                    if (msg == null) continue;
                    Send(msg);
                }
            }
            catch (Exception e) { log.Error("Erreur", e); }
            finally { client.Close(); }
        }

        void Send(string msg)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(msg);
                if (stream == null) return;
                stream.Write(data);
                log.Info("Envoyé : " + msg);
            }
            catch { running = false; }
        }

        void Read()
        {
            try
            {
                var buf = new byte[1024];
                while (running)
                {
                    if (stream == null) break;
                    int n = stream.Read(buf);
                    log.Warn("Serveur: " + Encoding.UTF8.GetString(buf, 0, n));
                }
            }
            catch { running = false; }
        }
    }
}
