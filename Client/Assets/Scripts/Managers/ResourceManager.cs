using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager 
{
	private static ResourceManager s_inst = null;
    public static ResourceManager Inst {  get 
        { 
            if (s_inst == null) s_inst = new ResourceManager(); 
            return s_inst; 
        } 
    }

    public T Load<T>(string _path) where T : Object
    {
        return Resources.Load<T>(_path);
    }

    public GameObject Instantiate(string _path, Transform _parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{_path}");
        if (prefab == null)
        {
            Debug.Log($"������ �ε� ���� : {_path}");
            return null;
        }
         
        GameObject go = Object.Instantiate(prefab, _parent);

        int idx = go.name.IndexOf("(Clone)");
        if (idx > 0) go.name = go.name.Substring(0, idx);

        return go;
    }

    public void Destroy(GameObject _go)
    {
        if (!_go) return;

        Object.Destroy(_go);
    }
}
