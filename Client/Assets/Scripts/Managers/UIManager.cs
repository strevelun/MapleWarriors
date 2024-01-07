using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using static Define;

public class UIManager
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
	// 씬 전환 시 밀기

	int m_order = 10;

	UIScene m_scene = null;
	Dictionary<string, UIPopup> m_dicPopup = new Dictionary<string, UIPopup>();
	Dictionary<string, UIPopup> m_dicPopupInDestructible = new Dictionary<string, UIPopup>();

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

	public void SetCanvas(GameObject _go, bool sort = true)
	{
		Canvas c = _go.GetComponent<Canvas>();
		c.overrideSorting = true;
		
		if (sort)
		{
			c.sortingOrder = m_order;
			++m_order;
		}
		else
			c.sortingOrder = 0;
	}

	public UIScene SetSceneUI(Define.Scene _sceneName)
	{
		string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();

		GameObject obj = ResourceManager.Inst.Instantiate("UI/Scene/" + curSceneType + "/UI" + _sceneName.ToString());
		m_scene = obj.GetComponent<UIScene>();
		return m_scene;
	}

	public UIPopup FindPopupUI(Define.UIPopup _prefabName)
	{
		string name = _prefabName.ToString();
		UIPopup popup;
		m_dicPopup.TryGetValue(name, out popup);
		if(!popup) m_dicPopupInDestructible.TryGetValue(name, out popup);
		if (!popup) return null;
		
		return popup;
	}


	public UIPopup AddPopupUI(Define.UIPopup _prefabName, bool _isOnScene = true)
	{
		string name = _prefabName.ToString();
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

		popup = ResourceManager.Inst.Instantiate(finalPath + name);
		uiPopup = popup.GetComponent<UIPopup>();

		if (_isOnScene)
			m_dicPopup.Add(name, uiPopup);
		else
		{
			m_dicPopupInDestructible.Add(name, uiPopup);
			uiPopup.SetDestroyOnLoad();
		}

		popup.transform.SetParent(Root.transform);
		popup.SetActive(false);
		return uiPopup;
	}

	public void ShowPopupUI(Define.UIPopup _prefabName)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		GameObject obj = popup.gameObject;
		SetCanvas(obj, true);
		obj.SetActive(true);
	}

	public void ShowPopupUI(Define.UIPopup _prefabName, string _description)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		GameObject obj = popup.gameObject;
		GameObject go = Util.FindChild(obj, true, "Content");
		TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
		if(tmp) tmp.text = _description;

		SetCanvas(obj, true);
		obj.SetActive(true);
	}

	public void HidePopupUI(Define.UIPopup _prefabName)
	{
		UIPopup popup = FindPopupUI(_prefabName);
		if (!popup) return;

		popup.gameObject.SetActive(false);
	}

	public void Clear()
	{
		foreach(UIPopup popup in m_dicPopup.Values)
		{
			ResourceManager.Inst.Destroy(popup.gameObject);
		}
		m_dicPopup.Clear();
	}
}
