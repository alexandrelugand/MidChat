# Guide des Processus et Threads en C#

## Introduction

Ce guide explique les concepts de **processus** et de **threads** dans le contexte Windows et C#. Les exemples sont basés sur le projet MidChat qui utilise le multithreading pour gérer plusieurs clients simultanément.

## Table des Matières

1. [Processus Windows](#processus-windows)
2. [Threads](#threads)
3. [Multithreading en C#](#multithreading-en-csharp)
4. [Exemple Pratique](#exemple-pratique)
5. [Bonnes Pratiques](#bonnes-pratiques)
6. [Alternatives Modernes](#alternatives-modernes)

---

## Processus Windows

### Qu'est-ce qu'un Processus ?

Un **processus** est une instance d'un programme en cours d'exécution. Chaque processus possède :

- **Espace mémoire propre** : Isolé des autres processus
- **Ressources système** : Fichiers, handles, connexions réseau
- **Au moins un thread** : Le thread principal
- **Code exécutable** : Le programme à exécuter

### Caractéristiques d'un Processus

```
┌─────────────────────────────────┐
│         PROCESSUS               │
│  ┌───────────────────────────┐  │
│  │   Espace Mémoire          │  │
│  │   (Heap, Stack, Code)     │  │
│  └───────────────────────────┘  │
│  ┌───────────────────────────┐  │
│  │   Threads                 │  │
│  │   • Thread Principal      │  │
│  │   • Thread 1              │  │
│  │   • Thread 2              │  │
│  └───────────────────────────┘  │
│  ┌───────────────────────────┐  │
│  │   Ressources              │  │
│  │   • Fichiers ouverts      │  │
│  │   • Connexions réseau     │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
```

### Processus en C#

```csharp
using System.Diagnostics;

// Obtenir le processus actuel
Process currentProcess = Process.GetCurrentProcess();
Console.WriteLine($"Nom du processus: {currentProcess.ProcessName}");
Console.WriteLine($"ID du processus: {currentProcess.Id}");
Console.WriteLine($"Mémoire utilisée: {currentProcess.WorkingSet64 / 1024 / 1024} MB");

// Lister tous les processus
Process[] processes = Process.GetProcesses();
foreach (var process in processes)
{
    Console.WriteLine($"{process.ProcessName} (PID: {process.Id})");
}

// Démarrer un nouveau processus
Process.Start("notepad.exe");
```

### Communication Inter-Processus

Les processus sont isolés, mais peuvent communiquer via :
- **Pipes** (Named Pipes, Anonymous Pipes)
- **Sockets** (TCP/IP, UDP)
- **Fichiers partagés**
- **Memory-Mapped Files**
- **Messages Windows**

---

## Threads

### Qu'est-ce qu'un Thread ?

Un **thread** (fil d'exécution) est la plus petite unité d'exécution dans un processus. Un processus peut avoir plusieurs threads qui s'exécutent en parallèle.

### Différence Processus vs Thread

| Aspect | Processus | Thread |
|--------|-----------|--------|
| **Mémoire** | Espace mémoire propre et isolé | Partage la mémoire du processus |
| **Création** | Coûteuse en ressources | Légère et rapide |
| **Communication** | Complexe (IPC) | Simple (mémoire partagée) |
| **Isolation** | Forte isolation | Pas d'isolation |
| **Contexte** | Changement de contexte coûteux | Changement de contexte moins coûteux |

### Représentation Visuelle

```
PROCESSUS
├── Thread Principal (Main)
├── Thread 1 (HandleClient pour Client 1)
├── Thread 2 (HandleClient pour Client 2)
└── Thread 3 (HandleClient pour Client 3)
     ↓
Tous partagent la même mémoire
```

---

## Multithreading en C#

### 1. Création de Threads avec la Classe Thread

```csharp
using System.Threading;

// Méthode à exécuter dans le thread
void WorkerMethod()
{
    Console.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine("Travail terminé");
}

// Créer et démarrer un thread
Thread thread = new Thread(WorkerMethod);
thread.Start();

// Attendre la fin du thread
thread.Join();
```

### 2. Thread avec Paramètres

```csharp
void WorkerWithParameter(object? obj)
{
    if (obj is string message)
    {
        Console.WriteLine($"Message: {message}");
    }
}

Thread thread = new Thread(WorkerWithParameter);
thread.Start("Hello from thread!");
```

### 3. Exemple du Projet MidChat

Dans le projet Server, chaque client est géré dans un thread séparé :

```csharp
while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Client connecté");
    
    // Création d'un nouveau thread pour chaque client
    var thread = new Thread(HandleClient);
    thread.Start(client);
}
```

**Pourquoi utiliser des threads ici ?**
- Le serveur peut gérer **plusieurs clients simultanément**
- Pendant qu'un thread attend des données d'un client, d'autres threads peuvent traiter d'autres clients
- Le thread principal reste disponible pour accepter de nouvelles connexions

### 4. États d'un Thread

```
┌─────────────┐
│   Unstarted │ ← Nouveau thread créé
└──────┬──────┘
       │ Start()
       ▼
┌─────────────┐
│   Running   │ ← Thread en exécution
└──────┬──────┘
       │
       ├──→ Sleep() ──→ WaitSleepJoin ──→ Running
       │
       ├──→ Wait() ────→ WaitSleepJoin ──→ Running
       │
       ▼
┌─────────────┐
│   Stopped   │ ← Thread terminé
└─────────────┘
```

### 5. Propriétés Importantes des Threads

```csharp
Thread thread = new Thread(WorkerMethod);

// Nom du thread (utile pour le débogage)
thread.Name = "WorkerThread1";

// Thread en arrière-plan (daemon)
thread.IsBackground = true; // Le processus peut se terminer sans attendre ce thread

// Priorité du thread
thread.Priority = ThreadPriority.Normal; // Lowest, BelowNormal, Normal, AboveNormal, Highest

thread.Start();
```

---

## Synchronisation des Threads

### Problème : Race Condition

Lorsque plusieurs threads accèdent à la même ressource :

```csharp
// ❌ DANGEREUX : Race condition
private static int counter = 0;

void IncrementCounter()
{
    for (int i = 0; i < 1000; i++)
    {
        counter++; // Pas thread-safe !
    }
}

// Deux threads peuvent lire la même valeur et écrire incorrectement
```

### Solution 1 : lock (Monitor)

```csharp
// ✅ SAFE : Utilisation de lock
private static int counter = 0;
private static object lockObject = new object();

void IncrementCounter()
{
    for (int i = 0; i < 1000; i++)
    {
        lock (lockObject)
        {
            counter++; // Thread-safe
        }
    }
}
```

### Solution 2 : Interlocked

```csharp
// ✅ SAFE : Opérations atomiques
private static int counter = 0;

void IncrementCounter()
{
    for (int i = 0; i < 1000; i++)
    {
        Interlocked.Increment(ref counter);
    }
}
```

### Solution 3 : Mutex

```csharp
// Pour synchroniser entre threads et processus
private static Mutex mutex = new Mutex();

void CriticalSection()
{
    mutex.WaitOne(); // Acquérir le mutex
    try
    {
        // Code critique
    }
    finally
    {
        mutex.ReleaseMutex(); // Libérer le mutex
    }
}
```

### Solution 4 : Semaphore

```csharp
// Limite le nombre de threads accédant à une ressource
private static Semaphore semaphore = new Semaphore(3, 3); // Max 3 threads

void AccessResource()
{
    semaphore.WaitOne(); // Acquérir un slot
    try
    {
        // Ressource limitée (ex: connexions DB)
    }
    finally
    {
        semaphore.Release(); // Libérer le slot
    }
}
```

---

## Exemple Pratique : Gestion Multi-Clients

### Architecture du Serveur MidChat

```csharp
public class ChatServer
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static object clientsLock = new object();
    
    public void Run()
    {
        const int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Serveur démarré sur le port {port}");
        
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connecté");
            
            lock (clientsLock)
            {
                clients.Add(client);
            }
            
            var thread = new Thread(HandleClient);
            thread.Name = $"ClientHandler-{clients.Count}";
            thread.IsBackground = true;
            thread.Start(client);
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
                            break;
                        
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] Reçu: {message}");
                        
                        // Diffuser à tous les clients
                        BroadcastMessage(message, tcpClient);
                    }
                }
            }
            finally
            {
                lock (clientsLock)
                {
                    clients.Remove(tcpClient);
                }
            }
        }
    }
    
    private static void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        
        lock (clientsLock)
        {
            foreach (var client in clients)
            {
                if (client != sender && client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        // Client déconnecté
                    }
                }
            }
        }
    }
}
```

### Flux d'Exécution

```
Thread Principal (Main)
    │
    ├─→ AcceptTcpClient() ─→ Client 1 connecté
    │        │
    │        └─→ Nouveau Thread ─→ HandleClient(Client 1)
    │                                    │
    │                                    └─→ Boucle de lecture
    │
    ├─→ AcceptTcpClient() ─→ Client 2 connecté
    │        │
    │        └─→ Nouveau Thread ─→ HandleClient(Client 2)
    │                                    │
    │                                    └─→ Boucle de lecture
    │
    └─→ AcceptTcpClient() ─→ Attend le prochain client...
```

---

## Bonnes Pratiques

### ✅ À Faire

1. **Nommer les threads pour le débogage**
   ```csharp
   thread.Name = "ClientHandler-1";
   ```

2. **Utiliser IsBackground pour les threads de service**
   ```csharp
   thread.IsBackground = true; // Ne bloque pas la fermeture de l'application
   ```

3. **Toujours synchroniser l'accès aux ressources partagées**
   ```csharp
   lock (lockObject)
   {
       // Accès à la ressource partagée
   }
   ```

4. **Gérer les exceptions dans les threads**
   ```csharp
   void ThreadMethod()
   {
       try
       {
           // Code du thread
       }
       catch (Exception ex)
       {
           Console.WriteLine($"Erreur dans le thread: {ex.Message}");
       }
   }
   ```

5. **Libérer les ressources correctement**
   ```csharp
   using (var resource = new Resource())
   {
       // Utilisation de la ressource
   }
   ```

### ❌ À Éviter

1. **Ne pas utiliser Thread.Abort()**
   - Obsolète et dangereux
   - Peut laisser l'application dans un état incohérent

2. **Ne pas bloquer le thread principal inutilement**
   ```csharp
   // ❌ Bloque l'UI
   Thread.Sleep(5000);
   
   // ✅ Utiliser un thread séparé
   Task.Run(() => Thread.Sleep(5000));
   ```

3. **Éviter les deadlocks**
   ```csharp
   // ❌ Risque de deadlock
   lock (obj1)
   {
       lock (obj2) { }
   }
   
   // Autre thread
   lock (obj2)
   {
       lock (obj1) { } // Deadlock!
   }
   ```

4. **Ne pas créer trop de threads**
   - Utiliser un ThreadPool ou Task Parallel Library
   - Trop de threads = overhead de gestion

---

## Alternatives Modernes

### 1. Task Parallel Library (TPL)

```csharp
// ✅ Approche moderne avec Task
Task.Run(() => HandleClient(client));

// Avec async/await
await Task.Run(() => DoWork());
```

### 2. Async/Await

```csharp
public async Task HandleClientAsync(TcpClient tcpClient)
{
    using (tcpClient)
    {
        var stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                break;
            
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Reçu: {message}");
        }
    }
}
```

### 3. ThreadPool

```csharp
// Utiliser le pool de threads géré par .NET
ThreadPool.QueueUserWorkItem(HandleClient, client);
```

### 4. Parallel LINQ (PLINQ)

```csharp
// Traitement parallèle de collections
var results = data.AsParallel()
                  .Where(x => x > 10)
                  .Select(x => x * 2)
                  .ToList();
```

### Comparaison : Thread vs Task

| Aspect | Thread | Task |
|--------|--------|------|
| **Niveau** | Bas niveau | Haut niveau |
| **Pool** | Non | Oui (ThreadPool) |
| **Gestion** | Manuelle | Automatique |
| **Async/Await** | Non | Oui |
| **Retour de valeur** | Difficile | Facile (Task<T>) |
| **Annulation** | Complexe | Simple (CancellationToken) |
| **Recommandé** | Cas spécifiques | Usage général |

---

## Thread-Safety et Collections

### Collections Non Thread-Safe

```csharp
// ❌ Pas thread-safe
List<string> list = new List<string>();
Dictionary<int, string> dict = new Dictionary<int, string>();
```

### Collections Thread-Safe (.NET)

```csharp
// ✅ Thread-safe
using System.Collections.Concurrent;

ConcurrentBag<string> bag = new ConcurrentBag<string>();
ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
ConcurrentStack<string> stack = new ConcurrentStack<string>();
ConcurrentDictionary<int, string> dict = new ConcurrentDictionary<int, string>();

// Exemple d'utilisation
dict.TryAdd(1, "value1");
dict.TryGetValue(1, out string? value);
dict.TryUpdate(1, "newValue", "value1");
dict.TryRemove(1, out string? removed);
```

---

## Débogage de Threads

### Fenêtre Threads dans Visual Studio

1. Pendant le débogage : **Debug → Windows → Threads**
2. Affiche tous les threads actifs
3. Permet de :
   - Voir l'état de chaque thread
   - Geler/dégeler des threads
   - Basculer entre threads

### Informations sur le Thread Actuel

```csharp
Thread currentThread = Thread.CurrentThread;
Console.WriteLine($"Thread ID: {currentThread.ManagedThreadId}");
Console.WriteLine($"Thread Name: {currentThread.Name}");
Console.WriteLine($"Is Background: {currentThread.IsBackground}");
Console.WriteLine($"Thread State: {currentThread.ThreadState}");
Console.WriteLine($"Priority: {currentThread.Priority}");
```

---

## Exemple Complet : Serveur Multi-Thread Amélioré

```csharp
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MidChat.Server
{
    internal class ImprovedChatServer
    {
        private static ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
        private static int clientCounter = 0;
        
        public void Run()
        {
            const int port = 5000;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Serveur démarré sur le port {port}");
            Console.WriteLine($"Thread Principal ID: {Thread.CurrentThread.ManagedThreadId}");
            
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                int clientId = Interlocked.Increment(ref clientCounter);
                
                clients.Add(client);
                Console.WriteLine($"Client {clientId} connecté (Total: {clients.Count})");
                
                var thread = new Thread(HandleClient)
                {
                    Name = $"ClientHandler-{clientId}",
                    IsBackground = true
                };
                thread.Start(new ClientInfo(client, clientId));
            }
        }
        
        private static void HandleClient(object? obj)
        {
            if (obj is not ClientInfo clientInfo)
                return;
            
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            
            try
            {
                using (clientInfo.Client)
                {
                    var stream = clientInfo.Client.GetStream();
                    byte[] buffer = new byte[1024];
                    
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;
                        
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] Reçu: {message}");
                        
                        BroadcastMessage($"Client {clientInfo.Id}: {message}", clientInfo.Client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Thread.CurrentThread.Name}] Erreur: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[{Thread.CurrentThread.Name}] Client {clientInfo.Id} déconnecté");
            }
        }
        
        private static void BroadcastMessage(string message, TcpClient sender)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            
            foreach (var client in clients)
            {
                if (client != sender && client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        // Client déconnecté
                    }
                }
            }
        }
        
        private record ClientInfo(TcpClient Client, int Id);
    }
}
```

---

## Ressources Supplémentaires

### Namespaces Importants
- `System.Threading` : Classes pour threads et synchronisation
- `System.Threading.Tasks` : Task Parallel Library
- `System.Collections.Concurrent` : Collections thread-safe
- `System.Diagnostics` : Processus et performances

### Documentation Microsoft
- [Thread Class](https://learn.microsoft.com/dotnet/api/system.threading.thread)
- [Task Parallel Library](https://learn.microsoft.com/dotnet/standard/parallel-programming/task-parallel-library-tpl)
- [Threading in C#](https://learn.microsoft.com/dotnet/standard/threading/)
- [Process Class](https://learn.microsoft.com/dotnet/api/system.diagnostics.process)

---

## Conclusion

### Points Clés à Retenir

1. **Processus** = Programme en exécution avec son propre espace mémoire
2. **Thread** = Unité d'exécution au sein d'un processus
3. **Multithreading** permet l'exécution parallèle pour améliorer les performances
4. **Synchronisation** est essentielle pour éviter les race conditions
5. **Task** est préféré à **Thread** pour le code moderne
6. Toujours utiliser `IsBackground = true` pour les threads de service
7. Gérer correctement les exceptions dans chaque thread

### Évolution Recommandée

```
Thread (ancien) → Task → async/await (moderne)
```

Pour les nouveaux projets, privilégiez **async/await** et **Task** plutôt que la classe **Thread** directe, sauf dans des cas spécifiques nécessitant un contrôle fin du thread.
