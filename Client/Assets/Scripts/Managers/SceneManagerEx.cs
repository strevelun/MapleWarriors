using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
	private static SceneManagerEx s_inst = null;
	public static SceneManagerEx Inst
	{
		get
		{
			if (s_inst == null) s_inst = new SceneManagerEx();
			return s_inst;
		}
	}

	public BaseScene CurScene { get; set; }

    public void LoadScene(Define.eScene _type)
    {
		CurScene.IsLoading = true;
        CurScene.Clear();
        SceneManager.LoadScene(GetSceneName(_type));
    }

	private string GetSceneName(Define.eScene _type)
    {
        return System.Enum.GetName(typeof(Define.eScene), _type);
    }
}
