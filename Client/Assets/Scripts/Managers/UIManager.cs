using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
	private static UIManager s_inst = null;
	public static UIManager Inst
	{
		get
		{
			if (s_inst == null)
			{
				s_inst = new UIManager();
				s_inst.ClearAll();
			}
			return s_inst;
		}
	}

	public UIScene SceneUI { private set; get; }
	private readonly Dictionary<Define.UIPopupEnum, UIPopup> m_dicPopup = new Dictionary<Define.UIPopupEnum, UIPopup>();
	private readonly Dictionary<Define.UIPopupEnum, UIPopup> m_dicPopupInDestructible = new Dictionary<Define.UIPopupEnum, UIPopup>();
	private readonly Dictionary<Define.UIChatEnum, UIChat> m_dicUIChat = new Dictionary<Define.UIChatEnum, UIChat>();
	private readonly Dictionary<Define.UIEnum, GameObject> m_dicUI = new Dictionary<Define.UIEnum, GameObject>();

	public GameObject Root
	{
		get
		{
			GameObject root = GameObject.Find("@UIRoot");
			if (!root)
			{
				root = new GameObject { name = "@UIRoot" };
				GameObject.DontDestroyOnLoad(root);
			}
			
			return root;
		}
	}

	public UIScene SetSceneUI(Define.SceneEnum _sceneName)
	{
		string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();

		GameObject obj = ResourceManager.Inst.Instantiate("UI/Scene/" + curSceneType + "/UI" + _sceneName.ToString());
		SceneUI = obj.GetComponent<UIScene>();
		return SceneUI;
	}

	#region PopupUI
	public UIPopup FindPopupUI(Define.UIPopupEnum _prefabName)
	{
		m_dicPopup.TryGetValue(_prefabName, out UIPopup popup);
		if(!popup) m_dicPopupInDestructible.TryGetValue(_prefabName, out popup);
		if (!popup) return null;
		
		return popup;
	}

	public UIPopup AddUI(Define.UIPopupEnum _prefabName, bool _isOnScene = true)
	{
		UIPopup uiPopup = FindPopupUI(_prefabName);
		if (uiPopup) return uiPopup;
		GameObject popup;

		string finalPath;

		if (_isOnScene)
		{
			string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();
			finalPath = "UI/Scene/" + curSceneType + "/Popup/";
		}
		else
		{
			finalPath = "UI/Popup/";
		}

		popup = ResourceManager.Inst.Instantiate(finalPath + _prefabName.ToString());
		uiPopup = popup.GetComponent<UIPopup>();

		if (_isOnScene)
			m_dicPopup.Add(_prefabName, uiPopup);
		else
		{
			m_dicPopupInDestructible.Add(_prefabName, uiPopup);
			uiPopup.SetDestroyOnLoad();
		}

		popup.transform.SetParent(Root.transform);
		popup.SetActive(false);
		return uiPopup;
	}

	public void ShowPopupUI(Define.UIPopupEnum _prefabName)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		GameObject obj = popup.gameObject;
		if (obj.activeSelf) return;

		obj.SetActive(true);
	}

	public void ShowPopupUI(Define.UIPopupEnum _prefabName, string _description)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		GameObject obj = popup.gameObject;
		if (obj.activeSelf) return;

		GameObject go = Util.FindChild(obj, true, "Content");
		TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
		if(tmp) tmp.text = _description;

		obj.SetActive(true);
	}

	public void HidePopupUI(Define.UIPopupEnum _prefabName)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		popup.gameObject.SetActive(false);
	}
	#endregion

	#region ChatUI
	public UIChat AddUI(Define.UIChatEnum _eChat)
	{
		if (m_dicUIChat.TryGetValue(_eChat, out UIChat chat)) return chat;

		string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();
		GameObject obj = ResourceManager.Inst.Instantiate("UI/Scene/" + curSceneType + "/" + _eChat.ToString(), UIManager.Inst.SceneUI.gameObject.transform);

		chat = obj.GetComponent<UIChat>();
		m_dicUIChat.Add(_eChat, chat);
		return chat;
	}

	public UIChat FindUI(Define.UIChatEnum _eChat)
	{
		if (!m_dicUIChat.TryGetValue(_eChat, out UIChat chat)) return null;

		return chat;
	}
	#endregion

	public GameObject AddUI(Define.UIEnum _eUI)
	{
		if (m_dicUI.ContainsKey(_eUI)) return null;

		string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();
		GameObject obj = ResourceManager.Inst.Instantiate("UI/Scene/" + curSceneType + "/" + _eUI.ToString(), SceneUI.gameObject.transform);

		m_dicUI.Add(_eUI, obj);
		return obj;
	}

	public GameObject AddUI(Define.UIEnum _eUI, GameObject _uiObj)
	{
		if (m_dicUI.TryGetValue(_eUI, out GameObject obj)) return null;

		m_dicUI.Add(_eUI, _uiObj);
		return obj;
	}

	public GameObject FindUI(Define.UIEnum _eUI)
	{
		if (!m_dicUI.TryGetValue(_eUI, out GameObject obj)) return null;

		return obj;
	}

	public void Clear()
	{
		m_dicPopup.Clear();
		m_dicUIChat.Clear();
		m_dicUI.Clear();
	}

	public void ClearAll()
	{
		Clear();
		m_dicPopupInDestructible.Clear();
	}
}
