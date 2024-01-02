using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UIManager
{
	// 씬 전환 시 밀기

	int m_order = 10;

	UIScene m_scene = null;
	Dictionary<string, GameObject> m_dicPopupPrefab = new Dictionary<string, GameObject>();

	public GameObject Root
	{
		get
		{
			GameObject root = GameObject.Find("@UIRoot");
			if (!root) root = new GameObject { name = "@UIRoot" };
			
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
		string curSceneType = Managers.Scene.CurScene.SceneType.ToString();

		GameObject obj = Managers.Resource.Instantiate("UI/Scene/" + curSceneType + "/UI" + _sceneName.ToString());
		m_scene = obj.GetComponent<UIScene>();
		return m_scene;
	}

	public GameObject FindPopupUI(Define.UIPopup _prefabName)
	{
		string name = _prefabName.ToString();
		GameObject popup;
		if (!m_dicPopupPrefab.TryGetValue(name, out popup)) return null;
		
		return popup;
	}

	public T AddPopupUI<T>(Define.UIPopup _prefabName) where T : UIPopup
	{
		string name = _prefabName.ToString();
		string curSceneType = Managers.Scene.CurScene.SceneType.ToString();
		GameObject popup = FindPopupUI(_prefabName);
		if (!popup)
		{
			popup = Managers.Resource.Instantiate("UI/Scene/" + curSceneType + "/Popup/" + name);
			m_dicPopupPrefab.Add(name, popup);
			popup.transform.SetParent(Root.transform);
		}
		popup.SetActive(false);
		return popup.GetComponent<T>();
	}

	public void ShowPopupUI(Define.UIPopup _prefabName)
	{
		GameObject popup = FindPopupUI(_prefabName);
		if (!popup) return;

		SetCanvas(popup, true);
		popup.SetActive(true);
	}

	public void ShowPopupUI(Define.UIPopup _prefabName, string _description)
	{
		GameObject popup = FindPopupUI(_prefabName);
		if (!popup) return;

		GameObject go = Util.FindChild(popup, true, "Content");
		TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
		if(tmp) tmp.text = _description;

		SetCanvas(popup, true);
		popup.SetActive(true);
	}

	public void HidePopupUI(Define.UIPopup _prefabName)
	{
		GameObject popup = FindPopupUI(_prefabName);
		if (!popup) return;

		popup.SetActive(false);
	}

	public void Clear()
	{
		foreach(GameObject obj in m_dicPopupPrefab.Values)
		{
			Managers.Resource.Destroy(obj);
		}
		m_dicPopupPrefab.Clear();
	}
}
