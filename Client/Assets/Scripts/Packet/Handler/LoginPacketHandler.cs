

using UnityEngine;

public static class LoginPacketHandler
{
	public static void LoginFailure(PacketReader _reader)
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopup.UILoginFailPopup_AlreadyLoggedIn);

	}

	public static void LoginSuccess(PacketReader _reader)
	{
		SceneManagerEx.Inst.LoadScene(Define.Scene.Lobby);

	}
}