using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Moonlapse.Networking
{
    public class Packet
    {
        [JsonProperty("a")]
        public string Action;

        [JsonProperty("p")]
        public List<object> Payloads;

        public Packet(string action = "", params object[] payloads)
        {
            Action = action;
            Payloads = new List<object>();

            if (payloads.Length != 0)
            {
                Payloads = new List<object>(payloads);
            }
        }

        public byte[] ToBytes()
        {
            var s = JsonConvert.SerializeObject(this);
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        public static Packet FromBytes(byte[] data)
        {
            var s = System.Text.Encoding.UTF8.GetString(data);
            var wrapper = JsonConvert.DeserializeObject<PacketWrapper>(s);

            return new Packet()
            {
                Action = wrapper.a,
                Payloads = wrapper.p
            };
        }

        public override string ToString()
        {
            return $"{Action}:{string.Join(",", Payloads)}";
        }

        public static Packet ConstructLoginPacket(string user, string pass)
        {
            return new Packet("Login", user, pass);
        }

        public static Packet ConstructLogoutPacket()
        {
            return new Packet("Logout");
        }

        public static Packet ConstructRegisterPacket(string user, string pass)
        {
            return new Packet("Register", user, pass);
        }

        public static Packet ConstructMovePacket(float dx, float dy)
        {
            return new Packet("Move", Math.Round(dx, 2), Math.Round(dy, 2));
        }

        public static Packet ConstructChatPacket(string type, string message)
        {
            return new Packet("Chat", type, message);
        }
    }

    public class PacketWrapper
    {
        public string a;
        public List<object> p;
    }
}
