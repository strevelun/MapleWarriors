using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager 
{
    public Action KeyAction = null; // listener 패턴. 키보드 입력을 감지하면 구독자들에게 뿌림

    public void OnUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.anyKey && KeyAction != null)      KeyAction.Invoke();


    }
}
