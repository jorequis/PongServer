using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PongServer
{
    class Player
    {
        public Player(UInt32 uid, IPEndPoint udp)
        {
            this.uid = uid;
            this.udp = udp;
        }

        public UInt32 uid;
        public IPEndPoint udp;
    }

    class Game
    {
        private MainActivity.LogDelegate logDelegate;

        public Game(Player player, MainActivity.LogDelegate logDelegate)
        {
            player1 = player;
            this.logDelegate = logDelegate;

            idToPlayer = new Dictionary<uint, int>();
            idToPlayer.Add(player.uid, 0);
        }

        private void Log(string text)
        {
            logDelegate(text);
        }

        private Player player1;
        private Player player2;

        private UdpClient udpOutput;

        public void Complete(Player player, UdpClient udpOutput)
        {
            player2 = player;
            idToPlayer.Add(player.uid, 1);

            this.udpOutput = udpOutput;

            udpOutput.Send(BitConverter.GetBytes((UInt32)153), 4, player1.udp);
            udpOutput.Send(BitConverter.GetBytes((UInt32)153), 4, player2.udp);

            new Thread(() => { Start(); }).Start();
        }

        //
        //
        //

        // Map Parameters
        public float halfMapWidth = 27f;
        public float halfMapHeight = 50f;

        // Player Parameters
        public float halfPlayerWidth = 6f;
        public float fullPlayerHeight = 3.2f;

        public float playerSpeed = 1.5f;

        public Dictionary<UInt32, int> idToPlayer;
        public Vector2[] playersPosition = new Vector2[] { new Vector2(0, 45), new Vector2(0, -45) };

        // Ball Parameters
        public float halfBallRadius = 1.2f;
        public float ballVelocity = 30f;
        public float ballAcceleration = 10f;
        public float ballAngleBounce = 60f;

        private float actualBallVelocity = 0;
        private Vector2 ballDirection;
        private Vector2 actualBallPosition;
        private bool behindPlayer = false;

        private float time;
        private UInt32 order;

        private Random rng;
        private int i = 0;

        private byte[] bytes;

        byte[] bytes_order;

        byte[] bytesX_ball;
        byte[] bytesY_ball;

        byte[] bytesX_p1;
        byte[] bytesY_p1;

        byte[] bytesX_p2;
        byte[] bytesY_p2;

        private void Start()
        {
            bytes = new byte[4 + (4 * 2) * 3];
            rng = new Random();
            ballDirection = Utils.RotateVector2d(new Vector2(0, rng.Next(0, 2) * 2 - 1), rng.Next(-15, 15));

            Update();
        }

        private void Update()
        {
            float deltaTime = 1 / 60f;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                deltaTime = watch.ElapsedMilliseconds / 1000f;
                watch.Restart();

                time += deltaTime;
                playersPosition[0].x = actualBallPosition.x;// (float)(Math.Sin(time) * 17);
                //playersPosition[1].x = (float)(Math.Sin(-time) * 17);

                actualBallPosition += ballDirection * actualBallVelocity * deltaTime;
                if (actualBallVelocity < ballVelocity) actualBallVelocity += ballAcceleration * deltaTime;

                //Si chocamos con un limite de altura se acaba el punto
                if (actualBallPosition.y - halfBallRadius < -halfMapHeight || actualBallPosition.y + halfBallRadius > halfMapHeight)
                {
                    int sign = Math.Sign(ballDirection.y);
                    actualBallPosition.y = sign * halfMapHeight - sign * halfBallRadius;
                    ballDirection.y = -ballDirection.y;
                    //actualBallVelocity += ballAcceleration;
                }

                if (actualBallPosition.y + halfBallRadius > playersPosition[0].y - fullPlayerHeight)
                    CheckBehindPlayer(0);
                else if (actualBallPosition.y - halfBallRadius < playersPosition[1].y + fullPlayerHeight)
                    CheckBehindPlayer(1);
                else if (behindPlayer)
                    behindPlayer = false;

                if (actualBallPosition.x - halfBallRadius < -halfMapWidth || actualBallPosition.x + halfBallRadius > halfMapWidth)
                {
                    int sign = Math.Sign(ballDirection.x);
                    actualBallPosition.x = sign * halfMapWidth - sign * halfBallRadius;
                    ballDirection.x = -ballDirection.x;
                    //actualBallVelocity += ballAcceleration;
                }

                SendData();
                Thread.Sleep(16);
            }
        }

        private void CheckBehindPlayer(int player)
        {
            if (behindPlayer)
            {
                /*if (actualBallPosition.x - halfBallRadius < playersPosition[player].x + halfPlayerWidth || actualBallPosition.x + halfBallRadius > playersPosition[player].x - halfPlayerWidth)
                {
                    int sign = Math.Sign(ballDirection.x);
                    actualBallPosition.x = playersPosition[player].x - sign * halfBallRadius;
                    ballDirection.x = -ballDirection.x;
                    //actualBallVelocity += ballAcceleration;
                }*/
            }
            else
            {
                behindPlayer = true;
                if (actualBallPosition.x - halfBallRadius < playersPosition[player].x + halfPlayerWidth && actualBallPosition.x + halfBallRadius > playersPosition[player].x - halfPlayerWidth)
                {
                    float quantity = (actualBallPosition.x - playersPosition[player].x) / halfPlayerWidth;
                    int sign = Math.Sign(ballDirection.y);
                    actualBallPosition.y = playersPosition[player].y - sign * fullPlayerHeight - sign * halfBallRadius;
                    Vector2 newDir = Utils.RotateVector2d(new Vector2(0, -sign), sign * ballAngleBounce * quantity);
                    ballDirection = newDir;
                }
            }
        }

        public void MovePlayer(UInt32 uid, int direction)
        {
            playersPosition[idToPlayer[uid]].x += (direction == 0 ? -1 : 1) * playerSpeed;
        }

        private void SendData()
        {
            bytes_order = BitConverter.GetBytes(order++);

            bytesX_ball = BitConverter.GetBytes(actualBallPosition.x);
            bytesY_ball = BitConverter.GetBytes(actualBallPosition.y);

            bytesX_p1 = BitConverter.GetBytes(playersPosition[0].x);
            bytesY_p1 = BitConverter.GetBytes(playersPosition[0].y);

            bytesX_p2 = BitConverter.GetBytes(playersPosition[1].x);
            bytesY_p2 = BitConverter.GetBytes(playersPosition[1].y);

            for (i = 0; i < 4; i++)
            {
                bytes[i] = bytes_order[i];

                bytes[i + 4] = bytesX_ball[i];
                bytes[i + 8] = bytesY_ball[i];

                bytes[i + 12] = bytesX_p1[i];
                bytes[i + 16] = bytesY_p1[i];

                bytes[i + 20] = bytesX_p2[i];
                bytes[i + 24] = bytesY_p2[i];
            }

            udpOutput.Send(bytes, bytes.Length, player2.udp);
        }
    }
}