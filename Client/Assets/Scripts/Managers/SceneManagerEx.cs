using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(Define.Scene _type)
    {
        CurScene.Clear();
        SceneManager.LoadScene(GetSceneName(_type));
    }

    string GetSceneName(Define.Scene _type)
    {
        return System.Enum.GetName(typeof(Define.Scene), _type);
    }
}
