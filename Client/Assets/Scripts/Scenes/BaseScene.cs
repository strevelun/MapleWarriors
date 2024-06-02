using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseScene : MonoBehaviour
{
	public bool IsLoading { get; set; } = false;
	public Define.SceneEnum SceneType { get; protected set; } = Define.SceneEnum.None;

	[SerializeField]
	private Image m_fadeInOut = null, m_loadingImage = null;

	protected float m_fadeInSecond = 0.5f, m_fadeOutSecond = 0.5f, m_loadingImageSecond = 1f;

	public virtual void Init()
	{
		SceneManagerEx.Inst.CurScene = FindObjectOfType<BaseScene>();
	}

	public virtual void Clear()
	{
		UIManager.Inst.Clear();
	}

	protected virtual void OnApplicationQuit()
	{
		UIManager.Inst.ClearAll();
		UDPCommunicator.Inst.Disconnect();

		Debug.Log("OnApplicationQuit");
	}

	protected void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
	}


	protected void StartFadeCoroutine()
	{
		if (m_loadingImage != null)
			StartCoroutine(LoadingCoroutine());
		else
			StartCoroutine(FadeInCoroutine());
	}

	protected IEnumerator LoadingCoroutine()
	{
		float sec = 0f;
		while(sec < m_loadingImageSecond)
		{
			sec += Time.deltaTime;
			yield return null;
		}

		m_loadingImage.gameObject.SetActive(false);
		StartCoroutine(FadeInCoroutine());
	}

	public void StartFadeInOutCoroutine(Action _action)
	{
		StartCoroutine(FadeInOutCoroutine(_action));
	}

	protected IEnumerator FadeInOutCoroutine(Action _action)
	{
		InputManager.Inst.SetInputEnabled(false);
		float fadeCnt = 0f;
		while (fadeCnt < m_fadeOutSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, fadeCnt / m_fadeOutSecond);
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 1);

		_action?.Invoke();
		yield return new WaitForSeconds(1f);

		fadeCnt = 0f;
		while (fadeCnt < m_fadeInSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, 1 - (fadeCnt / m_fadeInSecond));
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 0);
		InputManager.Inst.SetInputEnabled(true);
		GameManager.Inst.StageLoading = false;
	}

	public void StartFadeInCoroutine()
	{
		StartCoroutine(FadeInCoroutine());
	}

	public void StartFadeOutCoroutine()
	{
		StartCoroutine(FadeOutCoroutine());
	}

	// 호출 전 인풋 잠금
	protected IEnumerator FadeOutCoroutine()
	{
		InputManager.Inst.SetInputEnabled(false);
		float fadeCnt = 0f;
		while(fadeCnt < m_fadeOutSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, fadeCnt / m_fadeOutSecond);
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 1);
	}

	// 호출 후 인풋 잠금 해제
	protected IEnumerator FadeInCoroutine()
	{
		float fadeCnt = 0f;
		while (fadeCnt < m_fadeInSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, 1 - (fadeCnt / m_fadeInSecond));
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 0);
		InputManager.Inst.SetInputEnabled(true);
	}
}
