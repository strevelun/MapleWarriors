using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseScene : MonoBehaviour
{
	public bool IsLoading { get; set; } = false;
	public Define.eScene SceneType { get; protected set; } = Define.eScene.None;

	[SerializeField]
	Image m_fadeInOut = null, m_loadingImage = null;

	protected float m_fadeInSecond = 0.5f, m_fadeOutSecond = 0.5f, m_loadingImageSecond = 1f;

	void Start()
	{
		
	}

	void Update()
	{

	}

	void OnDestroy()
	{
		
	}

	protected virtual void Init()
	{
		UIManager.Inst.ClearAll();
		SceneManagerEx.Inst.CurScene = FindObjectOfType<BaseScene>();
	}

	public virtual void Clear()
	{
		UIManager.Inst.Clear();
	}

	protected virtual void OnApplicationQuit()
	{
		Packet pkt = LoginPacketMaker.ExitGame();
		NetworkManager.Inst.Send(pkt);

		UIManager.Inst.ClearAll();

		Debug.Log("OnApplicationQuit");
	}

	protected void StartFadeCoroutine()
	{
		if (m_loadingImage != null)
		{
			StartCoroutine(LoadingCoroutine());
		}
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

	public void StartFadeInOutCoroutine()
	{
		StartCoroutine(FadeInOutCoroutine());
	}

	protected IEnumerator FadeInOutCoroutine()
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

		yield return new WaitForSeconds(1f);

		fadeCnt = 0f;
		while (fadeCnt < m_fadeInSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, 1 - (fadeCnt / m_fadeInSecond));
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 0);
		//Debug.Log("FadeInOut");
		InputManager.Inst.SetInputEnabled(true);
	}

	public void StartFadeOutCoroutine(Define.eScene _type)
	{
		InputManager.Inst.SetInputEnabled(false);
		StartCoroutine(FadeOutCoroutine(_type));
	}

	// 호출 전 인풋 잠금
	protected IEnumerator FadeOutCoroutine(Define.eScene _type)
	{
		float fadeCnt = 0f;
		while(fadeCnt < m_fadeOutSecond)
		{
			fadeCnt += Time.deltaTime;
			m_fadeInOut.color = new Color(0, 0, 0, fadeCnt / m_fadeOutSecond);
			yield return null;
		}
		m_fadeInOut.color = new Color(0, 0, 0, 1);
		SceneManagerEx.Inst.LoadScene(_type);
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
