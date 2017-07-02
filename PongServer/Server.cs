using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace PongServer
{
    class Server
    {
        private MainActivity.LogDelegate logDelegate;

        private Random rng;
        private UInt32 nextUID;
        private Dictionary<UInt32, Game> idToGame;
        private List<Game> games;
        private List<Game> waitingGames;

        private UdpClient udpOutput;

        public Server(MainActivity.LogDelegate logDelegate)
        {
            this.logDelegate = logDelegate;

            rng = new Random();
            nextUID = 0;
            idToGame = new Dictionary<uint, Game>();
            games = new List<Game>();
            waitingGames = new List<Game>();

            new Thread(StartTCP).Start();
            new Thread(StartUDP).Start();

            //new Thread(Debug).Start();
        }

        private void Log(string text)
        {
            logDelegate(text);
        }

        public void Debug()
        {
            UdpClient udpServer = new UdpClient(9001);

            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, 9001);
                var data = udpServer.Receive(ref remoteEP); // listen on port 11000
                Log("receive data from " + remoteEP.ToString());
                Thread.Sleep(2000);
                Log("send data to " + remoteEP.ToString());
                udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
            }
        }
        
        public void StartTCP()
        {
            try
            {
                string log = "Starting TCP Listener..." + "\n";
                TcpListener serverSocket = new TcpListener(IPAddress.Any, 9000);
                serverSocket.Start(int.MaxValue - 1);
                log += "Listening on port: " + ((IPEndPoint)serverSocket.LocalEndpoint).Port + "\n";
                log += "──────────────────────────────────────────────────";
                Log(log);

                while (true)
                {
                    Socket conexionSocket = serverSocket.AcceptSocket();

                    byte[] send = BitConverter.GetBytes(nextUID);

                    for (int i = 0; i < 4; i++)
                        Log(Convert.ToString(send[i], 2).PadLeft(8, '0'));

                    conexionSocket.Send(send);
                    Log("New player: " + nextUID);
                    Log("──────────────────────────────────────────────────");
                    nextUID = nextUID == UInt32.MaxValue ? 0 : nextUID + 1;
                }
            }
            catch (Exception error)
            {
                Log("\n\nError: " + error.ToString() + "\n\n");
            }
        }

        public void StartUDP()
        {
            string log = "Starting UDP General..." + "\n";
            udpOutput = new UdpClient(9050);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 9050);
            log += "Listening on port: " + remoteEP.Port + "\n";
            log += "──────────────────────────────────────────────────";
            Log(log);

            while (true)
            {
                byte[] data = udpOutput.Receive(ref remoteEP);
                UInt32 uid = BitConverter.ToUInt32(data, 0);
                Game game;
                if (idToGame.TryGetValue(uid, out game))
                    game.MovePlayer(uid, ((int)data[4]));
                else
                    new Thread(() => { ProcessNewPlayer(uid, remoteEP); }).Start();
            }
        }

        public void ProcessNewPlayer(UInt32 uid, IPEndPoint udp)
        {
            if(waitingGames.Count > 0)
            {
                Log("Joning game: " + uid);
                Player player = new Player(uid, udp);
                Game game = waitingGames[rng.Next(0, waitingGames.Count)];

                idToGame.Add(player.uid, game);
                game.Complete(player, udpOutput);

                waitingGames.Remove(game);
                games.Add(game);
            }
            else
            {
                Log("Creating game: " + uid);
                Player player = new Player(uid, udp);
                Game game = new Game(player, logDelegate);

                idToGame.Add(player.uid, game);
                waitingGames.Add(game);
            }
        }

        public enum Funciones {  }

        public void Process(string a)
        {
            string[] argv = a.Split('/');

            Funciones func = (Funciones) int.Parse(argv[0]);
            
            switch (func)
            {
                default:
                    break;
            }
        }
    }
}