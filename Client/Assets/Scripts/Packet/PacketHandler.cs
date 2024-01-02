using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PacketType;

namespace Login
{
    public static class PacketHandler
    {
        public static void Handle(PacketReader _reader)
        {
            Server type = _reader.GetPacketType();
            switch(type)
            {
                case Server.LoginReqOK:
                    LoginReqOK(_reader);
                    break;
                case Server.LoginReqFail:
                    LoginReqFail(_reader);
                    break;
            }
        }
        
        private static void LoginReqOK(PacketReader _reader)
        {

        }

		private static void LoginReqFail(PacketReader _reader)
		{

		}
	}
}

namespace Lobby
{
    public static class PacketHandler
    {

    }
}