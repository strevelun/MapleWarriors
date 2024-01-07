using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketHandler
{
    public static void Handle(PacketReader _reader)
    {
		PacketType.Server type = _reader.GetPacketType();
		Debug.Log(type);

		switch (type)
        {
				#region Login
            case PacketType.Server.LoginFailure:
				LoginPacketHandler.LoginFailure(_reader);
                break;
			case PacketType.Server.LoginSuccess:
				LoginPacketHandler.LoginSuccess(_reader);
				break;
				#endregion
				#region Lobby
				#endregion
		}
	}

}