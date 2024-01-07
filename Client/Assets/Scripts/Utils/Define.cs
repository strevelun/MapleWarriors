using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
	public const int BufferMax = 1024;
	public const int PacketBufferMax = 128;
	public const int PacketHeaderSize = 4;
	public const int PacketSize = 2;
	public const int PacketType = 2;

	public enum Scene
	{
		None,
		Login,
		Lobby,
		Room,
		Ingame
	}

	public enum UIPopup
	{
		UILoginFailPopup_WrongInput,
		UILoginFailPopup_AlreadyLoggedIn,
		UIConnectFailPopup,
		UIDisconnectPopup,
	}
}
