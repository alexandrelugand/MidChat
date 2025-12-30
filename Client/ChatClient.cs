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

            using (var tcpClient = new TcpClient()) //Socket (point d'entrée du réseau / porte d'entrée / sortie vers le réseau)
            {
                tcpClient.Connect(serverIp, serverPort); //Etablissement de la connexion avec le serveur
                Console.WriteLine("Connecté au serveur de chat.");

                //Flux réseau
                var stream = tcpClient.GetStream(); //Canal ce communication bi-directionnel => Read / Write (forme de buffer réseau)

                while (true)
                { 
                    var msg = Console.ReadLine(); //Lecture de la chaine de caractère de la console (entréé par l'utilisateur)

                    if (string.IsNullOrEmpty(msg))
                        continue;

                    if (string.Compare(msg, "exit", StringComparison.OrdinalIgnoreCase) == 0) //msg == "exit" ou msg == "EXIT"
                        break;

                    byte[] data = Encoding.ASCII.GetBytes(msg); //Converti la chaine de caractère en tableau d'octets
                    stream.Write(data, 0, data.Length); //J'écris le tableau d'octets dans le stream réseau => sera envoyé au serveur
                    stream.Flush(); // => Maintenant, je l'envoie dans le réseau

                    byte[] buffer = new byte[1024]; //J'alloue un buffer d'octets de 1024 octets
                    int bytesRead = stream.Read(buffer, 0, buffer.Length); //Je lis les octets disponibles dans le stream (bloque s'il n'y en a pas), retourne le nombre d'octets lus
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead); //Je transforme les octets en une chaine de caractères lisibles
                    Console.WriteLine($"Serveur: {response}"); // Et je l'affiche dans la console
                }
            }
        }
    }
}
