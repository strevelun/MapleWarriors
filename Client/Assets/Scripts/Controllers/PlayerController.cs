using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class PlayerController : CreatureController
{
	public enum eState
	{
		None,
		Idle,
		Walking,
		Attack
	}

	public Skill CurSkill { get; protected set; } = null;

	GameObject m_tombstoneAnimObj;

	GameObject m_skillAnimObj;
	public Animator SkillAnim { get; private set; }
	GameObject m_rangedSkillAnimObj;
	public Animator RangedSkillAnim { get; private set; }

	protected eSkill m_eCurSkill = eSkill.None;

	TextMeshProUGUI m_nicknameTMP;
	TextMeshProUGUI m_positionTMP;
	string m_strNickname;

	[SerializeField]
	Vector2 m_nameTagOffset = new Vector2(0.5f, 1.5f); 
	RectTransform m_nameTagUI;
	[SerializeField]
	Vector2 m_positionTagOffset = new Vector2(0.5f, 2f);
	RectTransform m_positionTagUI;

	GameObject m_hpbarObj;
	Slider m_hpBarSlider;
	RectTransform m_sliderRect;

	TextMeshProUGUI m_hpbarText;

	protected GameObject m_nickname;

	public GameObject HitObj { get; private set; }

	// 3칸이면 1.5
	// 2칸이면 1
	[SerializeField]
	Vector2 m_hpBarUIOffset;

	//int m_moveCnt = 0;
	Coroutine m_movingCoroutine = null;

	void Start()
	{
		
	}

	protected override void Update()
	{
		base.Update();

		m_sliderRect.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_hpBarUIOffset);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

	

		m_positionTMP.text = $"x = {transform.position.x}, y = {transform.position.y}";

		m_nameTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_nameTagOffset);
		m_positionTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_positionTagOffset);
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		MaxSpeed = 4f;
		MaxHP = 50;
		HP = MaxHP;
		AttackDamage = 5;
		AttackRange = 2;

		HitboxWidth = 1;
		HitboxHeight = 1;

		//m_eCurSkill = eSkill.Slash;
		m_eCurSkill = eSkill.Slash;
		CurSkill = new Skill(m_eCurSkill);

		m_skillAnimObj = Util.FindChild(gameObject, true, "Skill");
		SkillAnim = m_skillAnimObj.transform.GetChild(0).GetComponent<Animator>();

		m_rangedSkillAnimObj = Util.FindChild(gameObject, true, "RangedSkill");
		RangedSkillAnim = m_rangedSkillAnimObj.transform.GetChild(0).GetComponent<Animator>();

		m_tombstoneAnimObj = Util.FindChild(gameObject, true, "Tombstone");
		m_tombstoneAnimObj.SetActive(false);

		GameObject playerUI = ResourceManager.Inst.Instantiate("Creature/UI/PlayerUI", gameObject.transform);

		m_damageObj = Util.FindChild(playerUI, true, "Damage");
		m_damageCanvasGroup = m_damageObj.GetComponent<CanvasGroup>();
		m_damageTMP = m_damageObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_damageTMP_RT = m_damageObj.transform.GetChild(0).GetComponent<RectTransform>();
		m_damageObj.SetActive(false);

		m_nickname = Util.FindChild(playerUI, true, "Nickname");
		m_nicknameTMP = m_nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_nameTagUI = m_nickname.GetComponent<RectTransform>();

		GameObject position = Util.FindChild(playerUI, true, "Position");
		m_positionTMP = position.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_positionTagUI = position.GetComponent<RectTransform>();

		m_hpbarObj = Util.FindChild(playerUI, true, "HPBar");
		m_hpBarSlider = Util.FindChild(m_hpbarObj, true, "Slider").GetComponent<Slider>();

		m_sliderRect = m_hpBarSlider.GetComponent<RectTransform>();
		m_sliderRect.sizeDelta = new Vector2(m_spriteRenderer.sprite.rect.width * transform.localScale.x, m_sliderRect.sizeDelta.y);

		m_hpbarText = Util.FindChild(m_hpbarObj, true, "HPText").GetComponent<TextMeshProUGUI>();

		m_hpBarSlider.maxValue = MaxHP;
		m_hpBarSlider.value = MaxHP;
		m_hpbarText.text = MaxHP.ToString();
		m_hpBarUIOffset = new Vector2(HitboxWidth / 2f, -0.2f);

		HitObj = Util.FindChild(gameObject, true, "Hit");
		HitObj.SetActive(false);

		ChangeState(new PlayerIdleState());

	}

	/*
public void UpdateSecondMove()
{
	float newX = transform.position.x, newY = transform.position.y;
	switch (SecondDir)
	{
		case eDir.Up:
			if (Dir != eDir.Down)
				newY = transform.position.y + (MaxSpeed * Time.fixedDeltaTime);
			break;
		case eDir.Down:
			if (Dir != eDir.Up)
				newY = transform.position.y - (MaxSpeed * Time.fixedDeltaTime);
			break;
		case eDir.Left:
			if (Dir != eDir.Right)
				newX = transform.position.x - (MaxSpeed * Time.fixedDeltaTime);
			break;
		case eDir.Right:
			if (Dir != eDir.Left)
				newX = transform.position.x + (MaxSpeed * Time.fixedDeltaTime);
			break;
	}

	transform.position = new Vector3(newX, newY);
	
}
	*/
	/*
	IEnumerator MovingCheckCoroutine()
	{
		while (true)
		{
			
			if (ByteDir == 0)
			{
				Debug.Log($"ByteDir = 0이라서 break : {m_moveCnt}");
				m_moveCnt = 0;
				break;
			}
			
			yield return new WaitForSeconds(0.2f);
			
			if(m_moveCnt == 0)
			{
				// 스탑!
				Debug.Log("MoveCnt is 0");
				ByteDir = 0;
				break;
			}

			--m_moveCnt;
			Debug.Log($"moveCnt 다운 : {m_moveCnt}");
		}
		m_movingCoroutine = null;
	}
*/
	public void Moving(float _xpos, float _ypos, byte _byteDir)
	{
		//++m_moveCnt;


		// moving으로 들어온 좌표가 우선
		float distX = Math.Abs(_xpos - transform.position.x);
		float distY = Math.Abs(_ypos - transform.position.y);

		//InGameConsole.Inst.Log($"Moving : {distX}, {distY}, 현재위치 : {transform.position.x}, {transform.position.y}, 패킷위치 : {_xpos}, {_ypos}");
		
		if(distX > MaxSpeed * 0.01f || distY > MaxSpeed * 0.01f)
		{
			transform.position = new Vector3(_xpos, _ypos);
			SetByteDir(_byteDir);
		//	if (m_movingCoroutine == null) m_movingCoroutine = StartCoroutine(MovingCheckCoroutine());
		}
	}

// 매 프레임마다 new
public void CheckMoveState()
	{
		if(CurState is PlayerIdleState)
		{
			if(Dir != eDir.None)
				ChangeState(new PlayerRunState());
			return;
		}
		if (CurState is PlayerRunState)
		{
			if (Dir == eDir.None)
				ChangeState(new PlayerIdleState());
			return;
		}	
	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
		m_nicknameTMP.text = m_strNickname;
	}

	public void BeginMovePosition(float _startXPos, float _startYPos, byte _byteDir)
	{
		//InGameConsole.Inst.Log($"BeginMove : {Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(_startXPos, _startYPos))}");
		InGameConsole.Inst.Log($"BeginMove : 현재 : {transform.position.x}, {transform.position.y}, 패킷 : {_startXPos}, {_startYPos}");

		//m_moveCnt = 1;
		//Debug.Log($"BeginMove : {m_moveCnt}");
		transform.position = new Vector3(_startXPos, _startYPos);
		SetByteDir(_byteDir);
		//if (m_movingCoroutine == null) m_movingCoroutine = StartCoroutine(MovingCheckCoroutine());
	}

	// _destXPos가 0이 나오는 경우
	public void EndMovePosition(float _destXPos, float _destYPos)
	{
		//Debug.Log($"EndMove : {m_moveCnt}");
		// CellPos갱신은 CreatureController에서 
		InGameConsole.Inst.Log($"EndMove : 현재 : {transform.position.x}, {transform.position.y}, 패킷 : {_destXPos}, {_destYPos}");

		SetByteDir(0);
		//eDir dir = GetDir(_byteDir);
		//Flip(dir);
		transform.position = new Vector3(_destXPos, _destYPos);

		//InGameConsole.Inst.Log($"EndMove : {Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(_destXPos, _destYPos))}");
	}

	public void PlayCurSkillAnim(Skill _skill)
	{
		eSkillType type = _skill.GetSkillType();
		if (type == eSkillType.Ranged)
		{
			RangedSkillAnim.Play(_skill.GetSkillName());
		}
		else
		{
			SkillAnim.Play(_skill.GetSkillName());
		}
	}

	public void Attack(byte _who, short _x, short _y)
	{
		if (!UserData.Inst.IsRoomOwner) return;

		eSkillType skillType = CurSkill.GetSkillType();
		if (skillType == eSkillType.Melee)
		{
			List<MonsterController> targets = new List<MonsterController>();
			bool activated = CurSkill.Activate(targets);
			if (activated)
			{
				ChangeState(new PlayerAttackState(CurSkill));
				foreach (MonsterController mc in targets)
					mc.Hit(CurSkill);
				Packet pkt = InGamePacketMaker.Attack(_who, targets, CurSkill.eCurSkill);
				UDPCommunicator.Inst.SendAll(pkt);
			}
		}
		else if (skillType == eSkillType.Ranged)
		{
			List<MonsterController> targets = new List<MonsterController>();
			bool activated = CurSkill.Activate(targets);
			if (activated)
			{
				ChangeState(new PlayerAttackState(CurSkill));
				foreach (MonsterController mc in targets)
					mc.Hit(CurSkill);
				Packet pkt = InGamePacketMaker.RangedAttack(_who, targets, CurSkill.eCurSkill, new Vector2Int(_x, _y));
				UDPCommunicator.Inst.SendAll(pkt);
			}
			SetRangedSkillObjPos(new Vector2Int(_x, _y));
		}
	}

	public void Hit(int _damage)
	{
		if (HP <= 0) return;

		HP -= _damage;
		if (HP < 0) HP = 0;
		m_hpbarText.text = HP.ToString();
		m_hpBarSlider.value -= _damage;

		m_damageObj.SetActive(true);
		m_damageCanvasGroup.alpha = 1f;

		m_damageTMP_RT.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_damageUIOffset);
		m_damageTMP.text = _damage.ToString();

		if (m_damamgeCoroutine != null)
		{
			StopCoroutine(m_damamgeCoroutine);
			m_damamgeCoroutine = null;
		}

		m_damamgeCoroutine = StartCoroutine(DamageCoroutine(1.0f));
	}

	public override void Die()
	{
		base.Die();

		m_tombstoneAnimObj.SetActive(true);

		GameManager.Inst.SubPlayerAliveCnt();

	}

	public override void Flip()
	{
		eSkillType type = CurSkill.GetSkillType();

		if (Dir == eDir.Right || Dir == eDir.DownRight) // 각 상하좌우는 시계방향 쪽 대각선 방향과 같도록
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0.5f, 0f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			m_skillAnimObj.transform.localScale = new Vector3(-1, 1, 1);
		}
		else if(Dir == eDir.Left || Dir == eDir.UpLeft)
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0.5f, 0f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			m_skillAnimObj.transform.localScale = new Vector3(1, 1, 1);
		}
		else if (Dir == eDir.Up || Dir == eDir.UpRight)
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0f, 1.5f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
			m_skillAnimObj.transform.localScale = new Vector3(1, 1, 1);
		}
		else if (Dir == eDir.Down || Dir == eDir.DownLeft)
		{
			m_skillAnimObj.transform.localPosition = new Vector3(1f, 0.2f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			m_skillAnimObj.transform.localScale = new Vector3(1, 1, 1);
		}
		base.Flip();
	}

	public void SetRangedSkillObjPos(Vector2Int _pos)
	{
		m_rangedSkillAnimObj.transform.localPosition = new Vector3(_pos.x, _pos.y);
	}

	public virtual void OnChangeStage()
	{
		ChangeState(new PlayerIdleState());

		m_spriteObject.SetActive(true);
		m_tombstoneAnimObj.SetActive(false);
		
		if(IsDead)
		{
			GameManager.Inst.AddPlayerAliveCnt();
		}

		HP = MaxHP;
		m_hpbarText.text = HP.ToString();
		m_hpBarSlider.value = MaxHP;
		ByteDir = 0;

	}
}
