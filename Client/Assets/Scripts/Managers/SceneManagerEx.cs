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

	public void LoadScene(Define.SceneEnum _type)
    {
		if(!CurScene.IsLoading)
		{
			CurScene.IsLoading = true;
			CurScene.Clear();
		}
        SceneManager.LoadScene(GetSceneName(_type));
    }

	public void LoadSceneWithFadeOut(Define.SceneEnum _type)
	{
		CurScene.IsLoading = true;
		CurScene.Clear();
		CurScene.StartFadeInOutCoroutine(() => LoadScene(_type));
	}

	private string GetSceneName(Define.SceneEnum _type)
    {
        return System.Enum.GetName(typeof(Define.SceneEnum), _type);
    }
}
