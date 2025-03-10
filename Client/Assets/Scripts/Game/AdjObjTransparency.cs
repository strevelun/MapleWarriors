﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjObjTransparency : MonoBehaviour
{
	private SpriteRenderer m_sr;

    void Start()
    {
        m_sr = GetComponent<SpriteRenderer>();
    }

	private void OnTriggerEnter2D(Collider2D _collision)
	{
        if (_collision.CompareTag("Player"))
            m_sr.color = new Color(1, 1, 1, 0.5f);
	}

	private void OnTriggerExit2D(Collider2D _collision)
	{
		if (_collision.CompareTag("Player"))
			m_sr.color = new Color(1, 1, 1, 1f);
	}
}
