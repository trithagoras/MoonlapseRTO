using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Moonlapse.Networking
{
    public static class NetworkState
    {
        public static int port = 42523;
        public static string host = "localhost";

        public static bool Running;

        private static TcpClient Client;

        public static Queue<Packet> Packets;

        public static bool Start()
        {
            if (Client != null && Client.Connected)
            {
                throw new Exception("Client already connected.");
            }

            Packets = new Queue<Packet>();

            try
            {
                Client = new TcpClient(host, port)
                {
                    ReceiveTimeout = 1000
                };
                Thread t = new Thread(new ThreadStart(ReadData));

                Running = true;
                t.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void ReadData()
        {
            while (Running)
            {
                var packet = ReadPacket();
                if (packet != null)
                {
                    Debug.Log("Received packet: " + packet);
                    Packets.Enqueue(packet);
                }
            }

            Client.GetStream().Close();
            Client.Close();
        }

        private static Packet ReadPacket()
        {
            byte[] buffer = new byte[1];
            var lenStr = "";

            while (Running)
            {
                try
                {
                    Client.GetStream().Read(buffer, 0, 1);
                }
                catch (System.IO.IOException)
                {
                    // squash (this is thrown on timeout)
                }

                var b = System.Text.Encoding.UTF8.GetString(buffer);

                if (b == "")
                {
                    // End of stream
                }

                else if (b == ":")
                {
                    try
                    {
                        var len = int.Parse(lenStr);
                        var data = new byte[len];
                        Client.GetStream().Read(data, 0, len);
                        Client.GetStream().Read(buffer, 0, 1);  // read trailing comma
                        return Packet.FromBytes(data);
                    }
                    catch (Exception)
                    {
                        // squash
                    }

                }

                else
                {
                    // byte is hopefully an int
                    try
                    {
                        int.Parse(b);
                        lenStr += b;
                    }
                    catch (Exception)
                    {
                        // squash
                    }
                }
            }
            return null;
        }

        private static string ToNetString(Packet p)
        {
            var s = p.ToBytes();
            return $"{s.Length}:{System.Text.Encoding.UTF8.GetString(s)},";
        }

        public static void SendPacket(Packet p)
        {
            var ns = ToNetString(p);
            var buff = System.Text.Encoding.UTF8.GetBytes(ns);

            Client.GetStream().Write(buff, 0, buff.Length);
        }

        public static void Stop()
        {
            Running = false;
            Client.GetStream().Close();
            Client.Close();
            Client = null;
        }
    }
}
