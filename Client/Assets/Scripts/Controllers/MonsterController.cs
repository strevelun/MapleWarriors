using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : CreatureController
{
	enum eState
	{
		None,
		Chase,
		Patrol,
		Attack
	}

	public bool AttackReady { get; set; } = true;

	AStar m_astar = new AStar();

	List<Vector2Int> m_path = null;
	Queue<PlayerController> m_targets = new Queue<PlayerController>();
	Queue<Vector2Int> m_dest = new Queue<Vector2Int>();
	Vector2Int m_prevTargetPos = new Vector2Int(-1,-1);
	bool m_targetMoved = false;
	//eState m_eState = eState.None;
	public int PathIdx { get; set; } = 0;
	public Vector2Int DestPos { get; set; } = new Vector2Int(0, 0);
	public bool CellArrived { get; private set; } = true;
	public int MaxHitPlayer { get; private set; }


	Coroutine m_damamgeCoroutine = null;

	GameObject m_damageObj;
	CanvasGroup m_damageCanvasGroup;
	TextMeshProUGUI m_damageTMP;
	RectTransform m_damageTMP_RT;

	[SerializeField]
	Vector2 m_damageUIOffset = new Vector2(0.5f, 1.5f);

	GameObject m_hpbarObj;
	Slider m_hpBarSlider;
	RectTransform m_sliderRect;

	TextMeshProUGUI m_hpbarText;

	// 3칸이면 1.5
	// 2칸이면 1
	[SerializeField]
	Vector2 m_hpBarUIOffset;


	void Start()
	{
		DestPos = CellPos;

		Init((int)transform.position.x, (int)transform.position.y);

		MonsterData monsterData = DataManager.Inst.FindMonsterData(gameObject.name);
		SetMonsterData(monsterData);
		ObjectManager.Inst.AddMonster(gameObject);
		MapManager.Inst.AddMonster(this);
	}

	protected override void Update()
	{
		base.Update();

		if (UserData.Inst.IsRoomOwner)
		{
			CheckTargetPosChanged();
			BeginSearch();
		}


		m_sliderRect.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_hpBarUIOffset);


	}


	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		UpdateChase();


	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		if (_cellYPos < 0) _cellYPos = -_cellYPos;

		base.Init(_cellXPos, _cellYPos);

		ChangeState(new MonsterIdleState());

		GameObject monsterUI = ResourceManager.Inst.Instantiate("Creature/UI/MonsterUI", gameObject.transform);

		m_damageObj = Util.FindChild(monsterUI, true, "Damage");
		m_damageCanvasGroup = m_damageObj.GetComponent<CanvasGroup>();
		m_damageTMP = m_damageObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_damageTMP_RT = m_damageObj.transform.GetChild(0).GetComponent<RectTransform>();
		m_damageObj.SetActive(false);

		m_hpbarObj = Util.FindChild(monsterUI, true, "HPBar");

		m_hpBarSlider = Util.FindChild(m_hpbarObj, true, "Slider").GetComponent<Slider>();

		m_sliderRect = m_hpBarSlider.GetComponent<RectTransform>();
		m_sliderRect.sizeDelta = new Vector2(m_spriteRenderer.sprite.rect.width * transform.localScale.x, m_sliderRect.sizeDelta.y);

		m_hpbarText = Util.FindChild(m_hpbarObj, true, "HPText").GetComponent<TextMeshProUGUI>();

		GameManager.Inst.AddMonsterCnt();

		StartCoroutine(ReadyForAttack());
	}

	public void SetMonsterData(MonsterData _data)
	{
		MaxSpeed = _data.speed;
		MaxHP = _data.HP;
		HP = _data.HP;
		AttackDamage = _data.attack;
		AttackRange = _data.attackCellRange;
		HitboxWidth = _data.hitboxWidth;
		HitboxHeight = _data.hitboxHeight;
		MaxHitPlayer = _data.maxHitPlayer;

		CircleCollider2D collider = GetComponent<CircleCollider2D>();
		if(collider == null) collider = gameObject.AddComponent<CircleCollider2D>();

		collider.offset = new Vector2(0.5f, 0.5f);
		collider.radius = _data.visionCellRange;

		m_hpBarSlider.maxValue = MaxHP;
		m_hpBarSlider.value = MaxHP;
		m_hpbarText.text = MaxHP.ToString();

		m_hpBarUIOffset = new Vector2(HitboxWidth / 2f, -0.3f);
	}

	public void CheckMoveState()
	{
		if (CurState is MonsterIdleState)
		{
			if (Dir != eDir.None)
				ChangeState(new MonsterRunState());
			return;
		}
		if (CurState is MonsterRunState)
		{
			if (Dir == eDir.None)
				ChangeState(new MonsterIdleState());
			return;
		}
	}

	void CheckTargetPosChanged()
	{
		if (m_targets.Count == 0) return;
		
		PlayerController pc = m_targets.Peek();
		if (m_prevTargetPos != pc.CellPos)
		{
			m_prevTargetPos = pc.CellPos;
			m_targetMoved = true;
		}
	}

	void BeginSearch()
	{
		if (m_targets.Count == 0) return;
		if (!m_targetMoved) return;
		if (!CellArrived) return;

		PlayerController pc = m_targets.Peek();
		if ((Math.Abs(CellPos.x - pc.CellPos.x) <= 1 && CellPos.y == pc.CellPos.y) ||
			(Math.Abs(CellPos.y - pc.CellPos.y) <= 1 && CellPos.x == pc.CellPos.x)) return;

		m_path = m_astar.Search(CellPos, pc.CellPos, HitboxWidth, HitboxHeight);
		if (m_path == null)
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			return;
		}

		if (m_path.Count <= 2)
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			return;
		}

		PathIdx = 1;

		Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
		NetworkManager.Inst.Send(pkt);

		m_targetMoved = false;
	}
	
	void UpdateChase()
	{
		if (CellArrived) return;
		if (Dir == eDir.None) return;
		//if (m_eState == eState.None) return;

		Vector2 dest = new Vector2(m_dest.Peek().x, -m_dest.Peek().y);
		float dist = Vector2.Distance(transform.position, dest);

		if (dist > MaxSpeed * Time.fixedDeltaTime * 2) return;

		MapManager.Inst.RemoveMonster(LastCellPos.x, LastCellPos.y, HitboxWidth, HitboxHeight);
		MapManager.Inst.AddMonster(this);
		transform.position = dest;
		m_dest.Dequeue();

		if (m_dest.Count > 0)
		{
			Vector2 dir = CellPos - new Vector2(m_dest.Peek().x, m_dest.Peek().y);

			if (dir.x <= -1) Dir = eDir.Right;
			else if (dir.x >= 1) Dir = eDir.Left;
			else if (dir.y <= -1) Dir = eDir.Down;
			else if (dir.y >= 1) Dir = eDir.Up;
		}
		else
		{
			Dir = eDir.None;
			//m_eState = eState.None;
			CellArrived = true;
		}

		if (UserData.Inst.IsRoomOwner)
		{
			if (m_targetMoved) return;

			if (m_path == null) return;
			if (PathIdx >= m_path.Count)
			{
				Dir = eDir.None;
				return;
			}

			++PathIdx;
				
			if (PathIdx < m_path.Count-1)
			{
				Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
				NetworkManager.Inst.Send(pkt);
			}
		}
	}
	

	public void BeginMove(int _pathIdx, int _cellXPos, int _cellYPos)
	{
		Vector2Int dest = new Vector2Int(_cellXPos, _cellYPos);
		if (!CanGo(CellPos, dest)) return;

		if (m_dest.Count == 0)
		{
			Vector2 dir = CellPos - dest;

			if (dir.x <= -1) Dir = eDir.Right;
			else if (dir.x >= 1) Dir = eDir.Left;
			else if (dir.y <= -1) Dir = eDir.Down;
			else if (dir.y >= 1) Dir = eDir.Up;
		}

		m_dest.Enqueue(dest);

		//m_eState = eState.Chase;

		CellArrived = false;
	}


	bool CanGo(Vector2Int _from, Vector2Int _to)
	{
		if (Math.Abs(_from.x - _to.x) + Math.Abs(_from.y - _to.y) != 1) return false;
		Vector2 dir = _from - _to;
		if (dir.normalized.x != -1 && dir.normalized.x != 1 && dir.normalized.y != -1 && dir.normalized.y != 1) return false;
		return true;
	}

	public void Hit(int _damage)
	{
		if (HP <= 0) return;

		//m_dest.Clear();

		HP -= _damage;
		m_hpbarText.text = HP.ToString();

		m_damageObj.SetActive(true);
		m_damageCanvasGroup.alpha = 1f;

		m_damageTMP_RT.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_damageUIOffset);
		m_damageTMP.text = _damage.ToString();

		m_hpBarSlider.value -= _damage;

		if (m_damamgeCoroutine != null)
		{
			StopCoroutine(m_damamgeCoroutine);
			m_damamgeCoroutine = null;
		}

		m_damamgeCoroutine = StartCoroutine(DamageCoroutine(1.0f));
	}

	IEnumerator DamageCoroutine(float _delay)
	{
		float elapsedTime = 0f;
		Vector3 startPos = m_damageTMP_RT.position;
		Vector3 destPos = startPos + new Vector3(0, 50, 0);
		float t = 0.0f;

		while (elapsedTime < _delay)
		{
			elapsedTime += Time.deltaTime;
			t = elapsedTime / _delay;

			m_damageTMP_RT.position = Vector3.Lerp(startPos, destPos, t);

			m_damageCanvasGroup.alpha = 1.0f - t;

			yield return null;
		}

		m_damageObj.SetActive(false);
	}

	public void Attack()
	{
		if (!AttackReady) return;
		if (m_targets.Count == 0) return;

		Queue<PlayerController> targets = new Queue<PlayerController>(m_targets);
		List<PlayerController> finalTargets = new List<PlayerController>();

		for (int i = 0; i < m_targets.Count; ++i)
		{
			PlayerController pc = targets.Peek();
			targets.Dequeue();

			Vector2Int dist = pc.CellPos - CellPos;
			int distX = Math.Abs(dist.x);
			int distY = Math.Abs(dist.y);

			if (distX <= AttackRange && distY <= AttackRange)
			{
				finalTargets.Add(pc);
				if (finalTargets.Count >= MaxHitPlayer) break;
			}
		}

		if (finalTargets.Count == 0) return;

		ChangeState(new MonsterAttackState(finalTargets));

	}

	public override void Die()
	{
		base.Die();

		gameObject.SetActive(false);
		GameManager.Inst.SubMonsterCnt();
	}

	public void DequeueTarget()
	{
		if (m_targets.Count == 0) return;

		m_targets.Dequeue();
	}

	IEnumerator ReadyForAttack()
	{
		while (true)
		{
			if (!AttackReady)
			{
				yield return new WaitForSeconds(3); 
				AttackReady = true;
			}
			else
			{
				yield return null;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D _other)
	{
		if (_other.gameObject.tag != "Player") return;

		PlayerController pc = _other.gameObject.GetComponent<PlayerController>();
		if (pc.IsDead) return;

		m_targets.Enqueue(pc);
	}

	void OnTriggerExit2D(Collider2D _other)
	{
		if (_other.gameObject.tag != "Player") return;

		m_targets.Dequeue();
		if (m_targets.Count == 0)
		{
			m_path = null;
			//Packet pkt = InGamePacketMaker.EndMoveMonster(name);
			//NetworkManager.Inst.Send(pkt);
		}
	}
}
