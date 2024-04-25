using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 1. 플레이어 발견하면 경로 생성
// 2. 경로의 첫번째 도착지점을 자신 포함 모두에게 보냄
// 3. 패킷 받은 정보를 토대로 한 칸 씩 이동


public class MonsterController : CreatureController
{
	enum eState
	{
		None,
		Chase,
		Patrol,
		Attack
	}

	bool[] m_playerEnter = new bool[4];

	public int Num { get; set; }

	public bool AttackReady { get; set; } = true;
	public int VisionCellRange { get; set; }

	AStar m_astar = new AStar();

	List<Vector2Int> m_path = null;
	List<PlayerController> m_targets = new List<PlayerController>();
	PlayerController m_target = null;

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
	[SerializeField]
	Vector2 m_locationInfoUIOffset = new Vector2(0.5f, -0.6f);

	GameObject m_locationInfoObj;
	RectTransform m_locationInfoRect;
	TextMeshProUGUI m_locationInfoText;

	bool m_beginMove = false;
	Vector2Int m_destReserved = new Vector2Int(0, 0);

	void Start()
	{
		
		Init((int)transform.position.x, (int)transform.position.y);




	}

	protected override void Update()
	{
		base.Update();

	

		if (UserData.Inst.IsRoomOwner)
		{
			PeekTarget();
			CheckTargetPosChanged();
			BeginSearch();
		}
		

		m_sliderRect.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_hpBarUIOffset);
		m_locationInfoRect.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_locationInfoUIOffset);

		//if(m_dest.Count != 0)
		//	Debug.Log($"{CellPos} -> {m_dest.Peek()}");
	}


	protected override void FixedUpdate()
	{
		base.FixedUpdate();


		BeginMove();
		UpdateMonsterMove();


	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		if (_cellYPos < 0) _cellYPos = -_cellYPos;

		base.Init(_cellXPos, _cellYPos);

		MonsterData monsterData = DataManager.Inst.FindMonsterData(gameObject.name);
		SetMonsterData(monsterData);

		ObjectManager.Inst.AddMonster(gameObject, monsterData.idx);
		MapManager.Inst.AddMonster(this);

		Idx = monsterData.idx;

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

		m_locationInfoObj = Util.FindChild(monsterUI, true, "LocationInfo");
		m_locationInfoRect = m_locationInfoObj.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
		m_locationInfoText = m_locationInfoObj.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

		MapManager.Inst.SetMonsterCollision(_cellXPos, _cellYPos, HitboxWidth, HitboxHeight, true);

		DestPos = CellPos;

		CircleCollider2D collider = GetComponent<CircleCollider2D>();
		if (collider == null) collider = gameObject.AddComponent<CircleCollider2D>();

		collider.offset = new Vector2(0.5f, 0.5f);
		collider.radius = VisionCellRange;

		m_hpBarSlider.maxValue = MaxHP;
		m_hpBarSlider.value = MaxHP;
		m_hpbarText.text = MaxHP.ToString();

		m_hpBarUIOffset = new Vector2(HitboxWidth / 2f, -0.3f);

		StartCoroutine(ReadyForAttack());
	}

	public void SetMonsterData(MonsterData _data)
	{
		Idx = _data.idx;
		MaxSpeed = _data.speed;
		MaxHP = _data.HP;
		HP = _data.HP;
		AttackDamage = _data.attack;
		AttackRange = _data.attackCellRange;
		VisionCellRange = _data.visionCellRange;
		HitboxWidth = _data.hitboxWidth;
		HitboxHeight = _data.hitboxHeight;
		MaxHitPlayer = _data.maxHitPlayer;
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

	void PeekTarget()
	{
		if (m_targets.Count == 0)
		{
			//ByteDir = 0;
			m_target = null;
			return;
		}

		int distCell = int.MaxValue;
		int distIdx = 0, tempIdx = 0;
		int distCellTemp;
		foreach(PlayerController pc in m_targets)
		{
			distCellTemp = Math.Abs(pc.CellPos.x - CellPos.x) + Math.Abs(pc.CellPos.y - CellPos.y);
			if (distCellTemp < distCell)
			{
				distIdx = tempIdx;
				distCell = distCellTemp;
			}
			++tempIdx;
		}
		m_target = m_targets[distIdx];
	}

	void CheckTargetPosChanged()
	{
		if (m_targets.Count == 0) return;
		
		if (m_prevTargetPos != m_target.CellPos)
		{
			m_prevTargetPos = m_target.CellPos;
			m_targetMoved = true;
		}
	}

	void BeginSearch()
	{
		if (m_targets.Count == 0) return;
		if (!m_targetMoved) return;
		if (!CellArrived) return;
		if (m_beginMove) return; // 이동 예약된 상태에선 경로 생성 금지
				
		if (Math.Abs(CellPos.x - m_target.CellPos.x) <= 1 && Math.Abs(CellPos.y - m_target.CellPos.y) <= 1) return;

		m_path = m_astar.Search(CellPos, m_target.CellPos, HitboxWidth, HitboxHeight, AttackRange);
		if (m_path == null)
		{
			ByteDir = 0;
			return;
		}
		
		if(m_path.Count > VisionCellRange * 2)
		{
			m_path = null;
			ByteDir = 0;
			return;
		}

		int closeDistLimit = HitboxWidth < HitboxHeight ? HitboxHeight : HitboxWidth;
		if (m_path.Count <= closeDistLimit)
		{
			ByteDir = 0;
			m_path = null;
			return;
		}

		PathIdx = 1;

		MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight, false);

		if (MapManager.Inst.IsMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, HitboxWidth, HitboxHeight))
		{
			MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight, true);
			return;
		}

		MapManager.Inst.SetMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, HitboxWidth, HitboxHeight, true);

		//Debug.Log($"SetMonsterCollision : {m_path[PathIdx].x}, {m_path[PathIdx].y}에 찜!");
		//Debug.Log($"Search : {m_path[PathIdx]}");

		Packet pkt = InGamePacketMaker.BeginMoveMonster(Idx, Num, m_path[PathIdx].x, m_path[PathIdx].y);
		UDPCommunicator.Inst.SendAll(pkt);
		ReserveBeginMove(m_path[PathIdx].x, m_path[PathIdx].y);

		m_targetMoved = false;
	}

	void UpdateMonsterMove()
	{
		if (CellArrived) return;

		float dist = Vector2.Distance(transform.position, new Vector3(DestPos.x, -DestPos.y));

		m_locationInfoText.text = $"{CellPos}, {DestPos}, dist : {dist}";

		MapManager.Inst.RemoveMonster(LastCellPos.x, LastCellPos.y, HitboxWidth, HitboxHeight);
		MapManager.Inst.AddMonster(this);

		if (dist > MaxSpeed * Time.fixedDeltaTime)
		{
			//if(dist > 1.5f)
				//Debug.Log($"dist : {dist}, DestPos : {DestPos}");
			return;
		}

		//Debug.Log($"{DestPos}로 바뀜");
		transform.position = new Vector3(DestPos.x, -DestPos.y);
		ByteDir = 0;
		CellArrived = true;

		Vector2 dir = CellPos - new Vector2(DestPos.x, DestPos.y);

		if (dir.x <= -1) ByteDir |= (byte)eDir.Right;
		if (dir.x >= 1) ByteDir |= (byte)eDir.Left;
		if (dir.y <= -1) ByteDir |= (byte)eDir.Down;
		if (dir.y >= 1) ByteDir |= (byte)eDir.Up;

		if (UserData.Inst.IsRoomOwner)
		{
			if (m_path == null) return;
			if (PathIdx >= m_path.Count)
			{
				ByteDir = 0;
				return;
			}

			++PathIdx;
				
			if (PathIdx < m_path.Count-1)
			{
				MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight, false);

				if (MapManager.Inst.IsMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, HitboxWidth, HitboxHeight))
				{
					m_targetMoved = true;
					MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight, true);
					return;
				}
				MapManager.Inst.SetMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, HitboxWidth, HitboxHeight, true);

				//Debug.Log($"update SetMonsterCollision : {m_path[PathIdx].x}, {m_path[PathIdx].y}에 찜!");

				Packet pkt = InGamePacketMaker.BeginMoveMonster(Idx, Num, m_path[PathIdx].x, m_path[PathIdx].y);
				UDPCommunicator.Inst.SendAll(pkt);
				ReserveBeginMove(m_path[PathIdx].x, m_path[PathIdx].y);
			}
		}
	}

	public void ReserveBeginMove(int _cellXPos, int _cellYPos)
	{
		m_destReserved = new Vector2Int(_cellXPos, _cellYPos);
		transform.position = new Vector3(DestPos.x, -DestPos.y);
		ByteDir = 0;
		CellArrived = true;
		//Debug.Log($"ReserveBeginMove : {DestPos}. {m_destReserved}");
		m_beginMove = true;
	}

	void BeginMove()
	{
		if (!m_beginMove) return;

		//Debug.Log($"BeginMove : {CellPos}, {m_destReserved}, {DestPos}");
		
		Vector2 dir = CellPos - m_destReserved;

		if (dir.x <= -1) ByteDir |= (byte)eDir.Right;
		if (dir.x >= 1) ByteDir |= (byte)eDir.Left;
		if (dir.y <= -1) ByteDir |= (byte)eDir.Down;
		if (dir.y >= 1) ByteDir |= (byte)eDir.Up;

		CellArrived = false;
		DestPos = m_destReserved;
		m_beginMove = false;
	}

	/*
	bool CanGo(Vector2Int _from, Vector2Int _to)
	{
		if (_from.x == _to.x && _from.y == _to.y) return false;
		if (Math.Abs(_from.x - _to.x) <= 1 && Math.Abs(_from.y - _to.y) <= 1) return true;
		return false;
	}
	*/
	public void Hit(Skill _skill)
	{
		if (!gameObject.activeSelf) return;
		if (HP <= 0) return;

		//m_dest.Clear();

		int damage = _skill.GetDamage();

		HP -= damage;
		if (HP < 0) HP = 0;

		m_hpbarText.text = HP.ToString();

		m_damageObj.SetActive(true);
		m_damageCanvasGroup.alpha = 1f;

		m_damageTMP_RT.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_damageUIOffset);
		m_damageTMP.text = damage.ToString();

		m_hpBarSlider.value -= damage;

		if (m_damamgeCoroutine != null)
		{
			StopCoroutine(m_damamgeCoroutine);
			m_damamgeCoroutine = null;
		}

		m_damamgeCoroutine = StartCoroutine(DamageCoroutine(1.0f));

		if (ChangeState(new MonsterHitState()) == false)
		{
			if (IsDead) ChangeState(new MonsterDeadState());
		}
	}

	public void Hit(int _hp) // 이 함수가 실행될때 이미 gameObject가 inactive
	{
		if (!gameObject.activeSelf) return;

		int damage = HP - _hp < 0 ? 0 : HP - _hp;
		HP = _hp;
		m_hpbarText.text = HP.ToString();

		m_damageObj.SetActive(true);
		m_damageCanvasGroup.alpha = 1f;

		m_damageTMP_RT.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_damageUIOffset);
		m_damageTMP.text = damage.ToString();

		m_hpBarSlider.value -= damage;

		if (m_damamgeCoroutine != null)
		{
			StopCoroutine(m_damamgeCoroutine);
			m_damamgeCoroutine = null;
		}

		m_damamgeCoroutine = StartCoroutine(DamageCoroutine(1.0f));

		if (ChangeState(new MonsterHitState()) == false)
		{
			if (IsDead) ChangeState(new MonsterDeadState());
		}
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
		if (!UserData.Inst.IsRoomOwner) return;
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

		Packet pkt = InGamePacketMaker.MonsterAttack(finalTargets, Idx, Num);
		UDPCommunicator.Inst.SendAll(pkt);

		MonsterController mc = ObjectManager.Inst.FindMonster(Idx, Num);
		if (mc) mc.ChangeState(new MonsterAttackState(finalTargets));
	}

	public override void Die()
	{
		base.Die();

		gameObject.SetActive(false);
		GameManager.Inst.SubMonsterCnt();

		if(Dir == eDir.None)
			MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight, false);
		else
			MapManager.Inst.SetMonsterCollision(DestPos.x, DestPos.y, HitboxWidth, HitboxHeight, false);
	}

	public void RemoveTarget(PlayerController _target)
	{
		if (m_targets.Count == 0) return;

		m_targets.Remove(_target);
	}

	IEnumerator ReadyForAttack()
	{
		while (true)
		{
			if (!UserData.Inst.IsRoomOwner)
			{
				yield return null;
				continue;
			}

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
		if (!UserData.Inst.IsRoomOwner) return;
		if (_other.gameObject.tag != "Player") return;

		PlayerController pc = _other.gameObject.GetComponent<PlayerController>();
		if (pc.IsDead) return;

		m_playerEnter[pc.Idx] = true;
		m_targets.Add(pc);
	}

	void OnTriggerStay2D(Collider2D _other)
	{
		if (!UserData.Inst.IsRoomOwner) return;
		if (_other.gameObject.tag != "Player") return;
		
		PlayerController pc = _other.gameObject.GetComponent<PlayerController>();
		if (m_playerEnter[pc.Idx] == true && pc.IsDead)
		{
			m_playerEnter[pc.Idx] = false;
			m_targets.Remove(pc);
			return;
		}
		if (m_playerEnter[pc.Idx] == true) return;
		if (pc.IsDead) return;

		m_playerEnter[pc.Idx] = true;
		m_targets.Add(pc);
	}

	// 몬스터가 셀과 셀 사이에 있을 때 플레이어가 exit하면 거기서 멈추게 하지 말것
	void OnTriggerExit2D(Collider2D _other)
	{
		if (!UserData.Inst.IsRoomOwner) return;
		if (_other.gameObject.tag != "Player") return;

		foreach(PlayerController pc in m_targets)
		{
			if(pc.name == _other.gameObject.name)
			{
				m_targets.Remove(pc);
				m_path = null;
				m_target = null;
				m_playerEnter[pc.Idx] = false;
				break;
			}
		}
	}
}
