using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MPL_server
{
    class Program
    {
        const int WELL_KNOWN_PORT = 2021;
        static int currentPortForClient = 2030;
        static List<LobbyConnection> lobbyClients;
        static List<string> partecipanti;
        static void Main(string[] args)
        {
            //all socket we need
            Socket lobby = new Socket(SocketType.Stream, ProtocolType.IP); //for the lobby
            lobbyClients = new List<LobbyConnection>();
            partecipanti = new List<string>();
            //for the player
            Socket player1 = new Socket(SocketType.Stream, ProtocolType.IP);
            Socket player2 = new Socket(SocketType.Stream, ProtocolType.IP);
            Socket player3 = new Socket(SocketType.Stream, ProtocolType.IP);
            Socket player4 = new Socket(SocketType.Stream, ProtocolType.IP);

            //your mother CANCELLAMI

            //metto la socket lobby in ascolto su 2021
            try
            {
                lobby.Bind(new IPEndPoint(IPAddress.Any, WELL_KNOWN_PORT));
                lobby.Listen(4);
                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    // Accetta una nuova connessione
                    Socket clnt_sck = lobby.Accept();

                    // Crea un nuovo thread per gestire la connessione
                    LobbyConnection cc = new LobbyConnection(clnt_sck);
                    lobbyClients.Add(cc);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        class ClientConnection
        {
            protected Socket _clnt_sck = null;
            protected Thread _tread = null;

            public ClientConnection(Socket clnt_sck)
            {
                _clnt_sck = clnt_sck;
                _clnt_sck.NoDelay = true;  // Disabilita l'algoritmo di Nagle
                                           // Vedi: https://en.wikipedia.org/wiki/Nagle%27s_algorithm

                _tread = new Thread(ThreadMain);
                _tread.Start();
            }
            protected virtual void ThreadMain()
            {
                Console.WriteLine("Handling client : {0}", _clnt_sck.RemoteEndPoint.ToString());

                while (true)
                {
                    // ...
                    // qui va il codice per ricevere le richieste del client ed inviare le relative risposte
                    // ...
                    // ...
                }

                Console.WriteLine("Closing client : {0}", _clnt_sck.RemoteEndPoint.ToString());

                _clnt_sck.Shutdown(SocketShutdown.Both);
                _clnt_sck.Close();
            }
        }

        class LobbyConnection : ClientConnection
        {
            public LobbyConnection(Socket clnt_sck) : base(clnt_sck) { }
            protected override void ThreadMain()
            {
                Console.WriteLine("Handling client : {0}", _clnt_sck.RemoteEndPoint.ToString());

                while (true)
                {
                    //qui va il codice per ricevere le richieste del client ed inviare le relative risposte della lobby
                    
                    byte[] buffer = new byte[1024]; // buffer di ricezione
                    const int N = 100; // quantità di dati da ricevere (N <= buffer.Length)
                    int j = 0;

                    int bytes = _clnt_sck.Receive(buffer);

                    //ci affidiamo al Dio cane per sapere se va CANCELLAMI
                    string received = Encoding.ASCII.GetString(buffer, 0, N);

                    System.Diagnostics.Debug.WriteLine(received);

                    string method = received.Split(' ')[0];
                    string nome = received.Split(' ')[1];

                    //la accetto solo se il nome è univoco e ho meno di 4 partecipanti
                    if (partecipanti.Count < 4 && !partecipanti.Contains(nome) && method == "INSERT")
                    {
                        partecipanti.Add(nome);
                        //dico che lho messo 
                        string resp = "INSERTOK " + currentPortForClient;
                        currentPortForClient++;
                        //convert to byte
                        byte[] toSend = Encoding.ASCII.GetBytes(resp);
                        for (int i = 0; i < toSend.Length;)
                            i += _clnt_sck.Send(toSend, i, toSend.Length - i, SocketFlags.None);
                        //diopporco CANCELLAMI
                    }

                }

                Console.WriteLine("Closing client : {0}", _clnt_sck.RemoteEndPoint.ToString());

                _clnt_sck.Shutdown(SocketShutdown.Both);
                _clnt_sck.Close();
            }
        }
    }
}
