using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static GameObject Find(string _name)
    {
        GameObject go = GameObject.Find(_name);
        if(!go)
        {
            Debug.Log("게임오브젝트 " + _name + "을 찾을 수 없음");
            return null;
        }
        return go;
	}

    public static GameObject FindChild(GameObject _parent, bool _recursive, string _name = null)
    {
        Transform child = FindChild<Transform>(_parent, _recursive, _name);
        if (child == null) return null;

        return child.gameObject;
    }

    public static T FindChild<T>(GameObject _parent, bool _recursive = false, string _name = null) where T : UnityEngine.Object
    {
        if (_parent == null) return null;

        if(_recursive)
        {
            foreach(T component in _parent.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(_name) || component.name == _name) return component;
            }
        }
        else
        {
            for(int i = 0; i < _parent.transform.childCount; ++i)
            {
                Transform t = _parent.transform.GetChild(i);
                if(string.IsNullOrEmpty(_name) || t.name == _name)
                {
                    T component = t.GetComponent<T>();
                    if (component) return component;
                }
            }
        }
        return null;
    }
}
