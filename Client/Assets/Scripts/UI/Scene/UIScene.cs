using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScene : MonoBehaviour
{
    Dictionary<string, GameObject> m_dicUI = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddUI(string _uiName)
    {
		GameObject obj = Util.FindChild(gameObject, false, _uiName);
        m_dicUI.Add(_uiName, obj);
	}

    public GameObject FindUI(string _uiName)
    {
        GameObject ui = null;
        m_dicUI.TryGetValue(_uiName, out ui);
        return ui;
    }
}
