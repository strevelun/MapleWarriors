using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_inst;
    static Managers Inst { get { Init(); return s_inst; } }

    InputManager m_input = new InputManager();
    ResourceManager m_resource = new ResourceManager();
    UIManager m_ui = new UIManager();
    SceneManagerEx m_scene = new SceneManagerEx();
    NetworkManager m_network = new NetworkManager();
    ActionQueue m_action = new ActionQueue();

    public static InputManager Input { get { return Inst.m_input; } }
    public static ResourceManager Resource { get { return Inst.m_resource; } }
    public static UIManager UI { get { return Inst.m_ui; } }
    public static SceneManagerEx Scene { get { return Inst.m_scene; } }
    public static NetworkManager Network { get { return Inst.m_network; } }
    public static ActionQueue Action { get { return Inst.m_action; } }

    void Start()
    {
        Init();
		Network.Connect("192.168.219.107", 30001);
	}

    void Update()
    {
        m_input.OnUpdate();
        m_action.Update();
    }

    static void Init()
    {
        if (s_inst) return;
        
        GameObject go = GameObject.Find("@Managers");
        if(go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
		s_inst = go.GetComponent<Managers>();
    }
}
