

using UnityEngine;

public static class LoginPacketHandler
{
	public static void LoginFailure_AlreadyLoggedIn(PacketReader _reader)
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UILoginFailPopup_AlreadyLoggedIn);

	}

	public static void LoginFailure_Full(PacketReader _reader)
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UILoginFailPopup_Full);
		Debug.Log("LoginFailure_Full");
	}

	public static void LoginSuccess(PacketReader _reader)
	{
		UserData.Inst.ConnectionID = _reader.GetUShort();
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.eScene.Lobby);
	}
}