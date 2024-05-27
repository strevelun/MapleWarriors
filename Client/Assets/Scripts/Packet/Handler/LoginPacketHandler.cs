using UnityEngine;

public static class LoginPacketHandler
{
	public static void LoginFailure_AlreadyLoggedIn()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UILoginFailPopup_AlreadyLoggedIn);
	}

	public static void LoginFailure_Full()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UILoginFailPopup_Full);
		Debug.Log("LoginFailure_Full");
	}

	public static void LoginSuccess()
	{
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.SceneEnum.Lobby);
	}

	public static void CheckedClientInfo()
	{
		LoginScene scene = SceneManagerEx.Inst.CurScene as LoginScene;
		scene.StopConnectToServerUDP();
		Debug.Log("CheckedClientInfo");
	}

	public static void ConnectionID(PacketReader _reader)
	{
		UserData.Inst.ConnectionID = _reader.GetUInt32();
		LoginScene scene = SceneManagerEx.Inst.CurScene as LoginScene;
		scene.StartConnectToServerUDP();
		Debug.Log($"ConnectionID : {UserData.Inst.ConnectionID}");
	}
}