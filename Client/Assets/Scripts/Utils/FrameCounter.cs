using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
	private float m_deltaTime = 0f;

	[SerializeField] private int m_size = 25;
	[SerializeField] private Color m_color = Color.red;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;
	}

	private void OnGUI()
	{
		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(30, 30, Screen.width, Screen.height);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = m_size;
		style.normal.textColor = m_color;

		float ms = m_deltaTime * 1000f;
		float fps = 1.0f / m_deltaTime;
		string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

		GUI.Label(rect, text, style);
	}
}
