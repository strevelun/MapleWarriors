using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class PlayerController : CreatureController
{
	public enum StateEnum
	{
		None,
		Idle,
		Walking,
		Attack
	}

	public int CharacterChoice { get; set; } = 0;

	public Skill CurSkill { get; protected set; } = null;

	private GameObject m_tombstoneAnimObj;

	private GameObject m_skillAnimObj;
	public Animator SkillAnim { get; private set; }
	private GameObject m_rangedSkillAnimObj;
	public Animator RangedSkillAnim { get; private set; }

	protected SkillEnum m_eCurSkill = SkillEnum.None;

	private TextMeshProUGUI m_nicknameTMP;
	private TextMeshProUGUI m_positionTMP;
	private string m_strNickname;

	private readonly Vector2 m_nameTagOffset = new Vector2(0.5f, 1.5f);
	private RectTransform m_nameTagUI;
	private readonly Vector2 m_positionTagOffset = new Vector2(0.5f, 2f);
	private RectTransform m_positionTagUI;

	private GameObject m_hpbarObj;
	private Slider m_hpBarSlider;
	private RectTransform m_sliderRect;

	private TextMeshProUGUI m_hpbarText;

	protected GameObject m_nickname;

	public GameObject HitObj { get; private set; }

	private readonly List<MonsterController> m_targets = new List<MonsterController>();

	[SerializeField]
	private Vector2 m_hpBarUIOffset;

	private void Start()
	{
		Init(Idx + 1, 1);
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

		SetPosition(_cellXPos, _cellYPos);

		//name = $"Player_{CharacterChoice}_{Idx}";

		MaxSpeed = 4f;
		CurSpeed = MaxSpeed;
		MaxHP = 60; // <= 65535
		HP = MaxHP;
		AttackDamage = 5;
		AttackRange = 2;

		HitboxWidth = 1;
		HitboxHeight = 1;

		m_eCurSkill = SkillEnum.Slash;
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

		m_startFontSize = m_damageTMP.fontSize;
		m_endFontSize = m_startFontSize * 3f;

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

		m_nicknameTMP.text = m_strNickname;

		ChangeState(new PlayerIdleState());
	}

	public void Moving(float _xpos, float _ypos, byte _byteDir)
	{
		float distX = Math.Abs(_xpos - transform.position.x);
		float distY = Math.Abs(_ypos - transform.position.y);

		if(distX > 0.1f || distY > 0.1f)
		{
			transform.position = Vector2.Lerp(new Vector2(transform.position.x, transform.position.y), new Vector2(_xpos, _ypos), 0.7f);
			SetByteDir(_byteDir);
		}

		if(IsDead)
		{
			transform.position = new Vector2(_xpos, _ypos);
		}
	}

	public void CheckMoveState()
	{
		if(CurState is PlayerIdleState)
		{
			if (Dir != DirEnum.None)
				ChangeState(new PlayerRunState());
			return;
		}
		if (CurState is PlayerRunState)
		{
			if (Dir == DirEnum.None)
				ChangeState(new PlayerIdleState());
			return;
		}	
	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
	}

	public void BeginMovePosition(byte _byteDir)
	{
		SetByteDir(_byteDir);
		//transform.position = new Vector2(_startXPos, _startYPos);
	}

	public void EndMovePosition(float _destXPos, float _destYPos)
	{
		SetByteDir(0);

		Vector2.Lerp(new Vector2(transform.position.x, transform.position.y), new Vector2(_destXPos, _destYPos), 0.7f);
	}

	public void PlayCurSkillAnim(Skill _skill)
	{
		SkillTypeEnum type = _skill.GetSkillType();
		if (type == SkillTypeEnum.Ranged)
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

		SkillTypeEnum skillType = CurSkill.GetSkillType();
		if (skillType == SkillTypeEnum.Melee)
		{
			m_targets.Clear();
			bool activated = CurSkill.Activate(m_targets);
			if (activated)
			{
				ChangeState(new PlayerAttackState(CurSkill));
				foreach (MonsterController mc in m_targets)
					mc.Hit(CurSkill);
				Packet pkt = InGamePacketMaker.Attack(_who, m_targets, CurSkill.CurSkill);
				UDPCommunicator.Inst.SendAll(pkt);
			}
		}
		else if (skillType == SkillTypeEnum.Ranged)
		{
			m_targets.Clear();
			bool activated = CurSkill.Activate(m_targets);
			if (activated)
			{
				ChangeState(new PlayerAttackState(CurSkill));
				foreach (MonsterController mc in m_targets)
					mc.Hit(CurSkill);
				Packet pkt = InGamePacketMaker.RangedAttack(_who, m_targets, CurSkill.CurSkill, new Vector2Int(_x, _y));
				UDPCommunicator.Inst.SendAll(pkt);
			}
			SetRangedSkillObjPos(new Vector2Int(_x, _y));
		}
	}

	public bool Hit(int _damage)
	{
		if (!gameObject.activeSelf) return false;
		if (_damage <= 0) return false;
		if (IsDead) return false;

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
			m_damageTMP.fontSize = m_startFontSize;
			m_damamgeCoroutine = null;
		}

		m_damamgeCoroutine = StartCoroutine(DamageCoroutine(1.0f));

		return true;
	}

	public override void Die()
	{
		base.Die();

		m_tombstoneAnimObj.SetActive(true);
		GameManager.Inst.SubPlayerAliveCnt();
	}

	public override void Flip()
	{
		if (Dir == DirEnum.Right || Dir == DirEnum.DownRight) // 각 상하좌우는 시계방향 쪽 대각선 방향과 같도록
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0.5f, 0f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			m_skillAnimObj.transform.localScale = new Vector3(-1, 1, 1);
		}
		else if(Dir == DirEnum.Left || Dir == DirEnum.UpLeft)
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0.5f, 0f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			m_skillAnimObj.transform.localScale = new Vector3(1, 1, 1);
		}
		else if (Dir == DirEnum.Up || Dir == DirEnum.UpRight)
		{
			m_skillAnimObj.transform.localPosition = new Vector3(0f, 1.5f);
			m_skillAnimObj.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
			m_skillAnimObj.transform.localScale = new Vector3(1, 1, 1);
		}
		else if (Dir == DirEnum.Down || Dir == DirEnum.DownLeft)
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
