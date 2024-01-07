using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager 
{
	private static UIManager s_inst = null;
	public static UIManager Inst
	{
		get
		{
			if (s_inst == null) s_inst = new UIManager();
			return s_inst;
		}
	}

	public void AddChatUI(UIChat _uiChat)
	{

	}

	public void AddChat(string _text)
	{
		// 게임오브젝트의 하위로 아이템 추카
	}
}
