# Guide des Connexions TCP/IP en C#

## Introduction

Ce guide explique comment implémenter des connexions TCP/IP en C# en utilisant les classes du namespace `System.Net.Sockets`. Les exemples sont basés sur le projet Server de MidChat.

## Concepts de Base

### Qu'est-ce que TCP/IP ?

**TCP/IP** (Transmission Control Protocol/Internet Protocol) est un protocole de communication réseau qui permet l'échange fiable de données entre deux machines sur un réseau.

- **TCP** : Assure une transmission fiable et ordonnée des données
- **IP** : Gère l'adressage et le routage des paquets

## Architecture Client-Serveur

### Le Serveur TCP

Un serveur TCP écoute les connexions entrantes sur un port spécifique et traite les demandes des clients.

#### 1. Configuration du Serveur

```csharp
using System.Net;
using System.Net.Sockets;

const int port = 5000;
TcpListener listener = new TcpListener(IPAddress.Any, port);
```

**Composants clés :**
- `TcpListener` : Classe qui écoute les connexions TCP entrantes
- `IPAddress.Any` : Accepte les connexions sur toutes les interfaces réseau disponibles
- `port` : Le numéro de port sur lequel le serveur écoute (5000 dans notre exemple)

#### 2. Démarrage du Serveur

```csharp
listener.Start();
Console.WriteLine($"Serveur démarré sur le port {port}");
```

La méthode `Start()` active l'écoute sur le port spécifié.

#### 3. Acceptation des Connexions Clientes

```csharp
while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Client connecté");
    
    var thread = new Thread(HandleClient);
    thread.Start(client);
}
```

**Fonctionnement :**
- `AcceptTcpClient()` : Méthode bloquante qui attend qu'un client se connecte
- Chaque client est géré dans un thread séparé pour permettre des connexions multiples
- Le serveur peut continuer à accepter de nouvelles connexions pendant le traitement

### Gestion des Clients

#### 1. Structure du Handler

```csharp
private static void HandleClient(object obj)
{
    if (obj is TcpClient tcpClient)
    {
        using (tcpClient)
        {
            // Traitement du client
        }
    }
}
```

**Points importants :**
- Le `using` statement assure la libération correcte des ressources réseau
- Le pattern matching vérifie que l'objet est bien un `TcpClient`

#### 2. Communication avec le Client

```csharp
var stream = tcpClient.GetStream();
byte[] buffer = new byte[1024];

while (true)
{
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
    if (bytesRead == 0)
        break; // Déconnexion du client
    
    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
    Console.WriteLine($"Reçu: {message}");
}
```

**Éléments clés :**
- `GetStream()` : Obtient le flux de données réseau
- `buffer` : Tableau d'octets pour stocker les données reçues (1024 bytes)
- `Read()` : Lit les données du flux (méthode bloquante)
- `bytesRead == 0` : Indique que le client s'est déconnecté
- `Encoding.ASCII.GetString()` : Convertit les octets en chaîne de caractères

## Classes Principales

### TcpListener

Écoute les connexions TCP entrantes.

**Méthodes importantes :**
- `Start()` : Démarre l'écoute
- `Stop()` : Arrête l'écoute
- `AcceptTcpClient()` : Accepte une connexion cliente (bloquant)
- `AcceptTcpClientAsync()` : Version asynchrone

### TcpClient

Représente une connexion TCP client-serveur.

**Méthodes importantes :**
- `GetStream()` : Obtient le NetworkStream pour lire/écrire
- `Connect()` : Se connecte à un serveur distant
- `Close()` : Ferme la connexion

### NetworkStream

Flux de données sur le réseau.

**Méthodes importantes :**
- `Read()` : Lit des données du flux
- `Write()` : Écrit des données dans le flux
- `ReadAsync()` / `WriteAsync()` : Versions asynchrones

## Bonnes Pratiques

### 1. Gestion des Ressources

```csharp
using (TcpClient client = listener.AcceptTcpClient())
{
    using (NetworkStream stream = client.GetStream())
    {
        // Utilisation du stream
    }
}
```

Toujours utiliser `using` pour garantir la libération des ressources réseau.

### 2. Multithreading

```csharp
var thread = new Thread(HandleClient);
thread.Start(client);
```

Gérer chaque client dans un thread séparé pour supporter plusieurs connexions simultanées.

### 3. Détection de Déconnexion

```csharp
int bytesRead = stream.Read(buffer, 0, buffer.Length);
if (bytesRead == 0)
    break; // Le client s'est déconnecté
```

Toujours vérifier si `Read()` retourne 0, ce qui indique une déconnexion propre.

### 4. Taille du Buffer

```csharp
byte[] buffer = new byte[1024];
string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
```

**Important :** Utilisez `bytesRead` lors du décodage, pas `buffer.Length`, pour éviter de lire des données invalides.

## Erreurs Courantes à Éviter

### ❌ Utiliser buffer.Length au lieu de bytesRead

```csharp
// INCORRECT
string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
```

Cela peut inclure des données résiduelles du buffer.

### ✅ Utiliser bytesRead

```csharp
// CORRECT
string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
```

### ❌ Ne pas gérer les exceptions réseau

Les opérations réseau peuvent lever des exceptions (timeout, déconnexion brutale, etc.).

### ✅ Ajouter une gestion d'erreurs

```csharp
try
{
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
    // Traitement
}
catch (IOException ex)
{
    Console.WriteLine($"Erreur de communication: {ex.Message}");
}
```

## Améliorations Possibles

### 1. Programmation Asynchrone

```csharp
public async Task RunAsync()
{
    while (true)
    {
        TcpClient client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleClient(client));
    }
}
```

### 2. Gestion d'une Liste de Clients

```csharp
private static List<TcpClient> clients = new List<TcpClient>();
```

Permet de diffuser des messages à tous les clients connectés.

### 3. Ajout de Timeouts

```csharp
tcpClient.ReceiveTimeout = 5000; // 5 secondes
tcpClient.SendTimeout = 5000;
```

## Exemple Complet : Client TCP

```csharp
using System.Net.Sockets;
using System.Text;

public class ChatClient
{
    public void Connect(string serverIp, int port)
    {
        using (TcpClient client = new TcpClient(serverIp, port))
        using (NetworkStream stream = client.GetStream())
        {
            string message = "Hello Server!";
            byte[] data = Encoding.ASCII.GetBytes(message);
            
            stream.Write(data, 0, data.Length);
            Console.WriteLine($"Envoyé: {message}");
        }
    }
}
```

## Ressources Supplémentaires

### Namespaces Importants
- `System.Net` : Classes pour les adresses IP et les endpoints
- `System.Net.Sockets` : Classes pour les sockets TCP/UDP
- `System.Text` : Encodage et décodage de texte

### Documentation Microsoft
- [TcpListener Class](https://learn.microsoft.com/dotnet/api/system.net.sockets.tcplistener)
- [TcpClient Class](https://learn.microsoft.com/dotnet/api/system.net.sockets.tcpclient)
- [NetworkStream Class](https://learn.microsoft.com/dotnet/api/system.net.sockets.networkstream)

## Conclusion

Les connexions TCP/IP en C# sont rendues simples grâce aux classes `TcpListener` et `TcpClient`. Les points clés à retenir :

1. **TcpListener** écoute les connexions entrantes
2. **TcpClient** représente une connexion active
3. **NetworkStream** permet la communication bidirectionnelle
4. Toujours gérer correctement les ressources avec `using`
5. Utiliser le multithreading pour supporter plusieurs clients
6. Vérifier `bytesRead` pour détecter les déconnexions

Ce guide fournit les bases pour créer des applications réseau robustes en C#.
