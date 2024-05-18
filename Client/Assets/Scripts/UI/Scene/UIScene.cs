using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScene : MonoBehaviour
{
	private readonly Dictionary<string, GameObject> m_dicUI = new Dictionary<string, GameObject>();

    public GameObject AddUI(string _uiName)
    {
		GameObject obj = Util.FindChild(gameObject, false, _uiName);
        m_dicUI.Add(_uiName, obj);
        return obj;
	}

    public GameObject FindUI(string _uiName)
    {
		m_dicUI.TryGetValue(_uiName, out GameObject ui);
		return ui;
	}
}
