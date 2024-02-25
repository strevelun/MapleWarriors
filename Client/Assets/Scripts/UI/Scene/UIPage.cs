using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPage : MonoBehaviour
{
	private List<GameObject> m_listContentItem = new List<GameObject>();

	public int ActiveItemCount { get; set; } = 0;

	private byte m_curPage = 0;
	public byte CurPage
	{
		get { return m_curPage; }
		set
		{
			if (value < MaxPage)
				m_curPage = value;
		}
	}
	public byte MaxPage { get; private set; }	
	public byte MaxItemInPage { get; private set; }	

	// SceneTypeÀ¸·Î
	public void Init(byte _maxPage, byte _maxItemInPage, string _itemPath, Define.eUI _eItem, string _parentOfItemName)
	{
		MaxPage = _maxPage;
		MaxItemInPage = _maxItemInPage;

		Transform parent = Util.FindChild(gameObject, false, _parentOfItemName).transform;
		GameObject instance;
		for (int i = 0; i < MaxItemInPage; ++i)
		{
			instance = ResourceManager.Inst.Instantiate(_itemPath + _eItem.ToString() + "Item", parent);
			m_listContentItem.Add(instance);
			instance.SetActive(false);
		}
	}

	public GameObject GetItem(int _idx) 
	{
		if (_idx < 0 || _idx >= m_listContentItem.Count) return null;

		return m_listContentItem[_idx];
	}
}
