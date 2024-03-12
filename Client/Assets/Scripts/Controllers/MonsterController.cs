using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

	public bool AttackReady { get; set; } = true;
	public int VisionCellRange { get; set; }

	AStar m_astar = new AStar();

	List<Vector2Int> m_path = null;
	List<PlayerController> m_targets = new List<PlayerController>();
	PlayerController m_target = null;

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
		Init((int)transform.position.x, (int)transform.position.y);

		DestPos = CellPos;

		MonsterData monsterData = DataManager.Inst.FindMonsterData(gameObject.name);
		SetMonsterData(monsterData);
		ObjectManager.Inst.AddMonster(gameObject);
		MapManager.Inst.AddMonster(this);
	}

	protected override void Update()
	{
		base.Update();

		PeekTarget();

		if (UserData.Inst.IsRoomOwner)
		{
			CheckTargetPosChanged();
			BeginSearch();
		}

		m_sliderRect.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_hpBarUIOffset);

		//if(m_dest.Count != 0)
		//	Debug.Log($"{CellPos} -> {m_dest.Peek()}");
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

		MapManager.Inst.SetMonsterCollision(_cellXPos, _cellYPos, true);
		//GameManager.Inst.AddMonsterCnt();

		StartCoroutine(ReadyForAttack());
	}

	public void SetMonsterData(MonsterData _data)
	{
		MaxSpeed = _data.speed;
		MaxHP = _data.HP;
		HP = _data.HP;
		AttackDamage = _data.attack;
		AttackRange = _data.attackCellRange;
		VisionCellRange = _data.visionCellRange;
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
				
		if (Math.Abs(CellPos.x - m_target.CellPos.x) <= 1 && Math.Abs(CellPos.y - m_target.CellPos.y) <= 1) return;

		m_path = m_astar.Search(CellPos, m_target.CellPos, HitboxWidth, HitboxHeight);
		if (m_path == null)
		{
			ByteDir = 0;
			//m_eState = eState.None;
			return;
		}
		if(m_path.Count > VisionCellRange)
		{
			m_path = null;
			ByteDir = 0;
			return;
		}

		if (m_path.Count <= 1 + AttackRange)
		{
			ByteDir = 0;
			//m_eState = eState.None;
			return;
		}

		PathIdx = 1;

		if (MapManager.Inst.IsMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y)) return;

		MapManager.Inst.SetMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, true);

		Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
		NetworkManager.Inst.Send(pkt);

		m_targetMoved = false;
		//Debug.Log($"BeginSearch : {m_path}");
	}
	
	void UpdateChase()
	{
		if (CellArrived) return;
		//if (Dir == eDir.None) return;
		//if (ByteDir == 0) return;
		//if (m_eState == eState.None) return;

		//Debug.Log($"{Dir}, {ByteDir}");

		Vector2 dest = new Vector2(m_dest.Peek().x, -m_dest.Peek().y);

		float dist = Vector2.Distance(transform.position, dest);

		//Debug.Log($"{dist} -> {transform.position} : {Dir}, 목적지 : {dest}");

		if (dist > MaxSpeed * Time.fixedDeltaTime * 2) return;

		MapManager.Inst.RemoveMonster(LastCellPos.x, LastCellPos.y, HitboxWidth, HitboxHeight);
		MapManager.Inst.AddMonster(this);
		transform.position = dest;
		m_dest.Dequeue();

		if(m_target == null)
		{
			m_dest.Clear();
		}


		if (m_dest.Count > 0)
		{
			Vector2Int vecDest = m_dest.Peek();
			Vector2 dir = CellPos - new Vector2(vecDest.x, vecDest.y);

			ByteDir = 0;

			if (dir.x <= -1) ByteDir |= (byte)eDir.Right;
			if (dir.x >= 1) ByteDir |= (byte)eDir.Left;
			if (dir.y <= -1) ByteDir |= (byte)eDir.Down;
			if (dir.y >= 1) ByteDir |= (byte)eDir.Up;

			MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, false);
			MapManager.Inst.SetMonsterCollision(vecDest.x, vecDest.y, true);
			Debug.Log($"{CellPos.x}, {CellPos.y} -> {vecDest.x}, {vecDest.y}");
			//Debug.Log("UpdateChase에서 방향전환");
		}
		else
		{
			ByteDir = 0;
			//m_eState = eState.None;
			Debug.Log($"{CellPos.x}, {CellPos.y} 도착");
		}
			
		CellArrived = true;

		if (UserData.Inst.IsRoomOwner)
		{
			if (m_targetMoved) return;

			if (m_path == null) return;
			if (PathIdx >= m_path.Count)
			{
				ByteDir = 0;
				return;
			}

			++PathIdx;
				
			if (PathIdx < m_path.Count-1)
			{
				if (MapManager.Inst.IsMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y))
				{
					// 경로 다시 생성해야
					m_targetMoved = true;
					return;
				}

				MapManager.Inst.SetMonsterCollision(m_path[PathIdx].x, m_path[PathIdx].y, true);

				Packet pkt = InGamePacketMaker.BeginMoveMonster(name, m_path[PathIdx].x, m_path[PathIdx].y, PathIdx);
				NetworkManager.Inst.Send(pkt);
			}
		}
	}
	

	public void BeginMove(int _pathIdx, int _cellXPos, int _cellYPos)
	{
		//Debug.Log($"BeginMove : {ByteDir}");
		Vector2Int dest = new Vector2Int(_cellXPos, _cellYPos);
		Vector2Int start = m_dest.Count == 0 ? CellPos : m_dest.Peek();
		if (!CanGo(start, dest))
		{
			// 만약 갈 수 없는 dest가 나오면 현재 m_Dest에 있는 경로 전부 삭제 후 경로 재설정해야
			m_targetMoved = true;
			if (m_dest.Count > 0)
			{
				Vector2Int curDest = m_dest.Peek();
				m_dest.Clear();
				m_dest.Enqueue(curDest);
			}
			Debug.Log($"{start}에서 {dest}로 갈 수 없음");
			return;
		}

		// 현재 셀에 도착하지 않을 수 있는 상태에서 방향 전환을 할 가능성이 있기 때문에 방향전환은 UpdateChase에서만
		
		if (m_dest.Count == 0)
		{
			Vector2 dir = CellPos - dest;

			if (dir.x <= -1) ByteDir |= (byte)eDir.Right;
			if (dir.x >= 1) ByteDir |= (byte)eDir.Left;
			if (dir.y <= -1) ByteDir |= (byte)eDir.Down;
			if (dir.y >= 1) ByteDir |= (byte)eDir.Up;

			transform.position = new Vector3(CellPos.x, -CellPos.y);

			//Debug.Log("BeginMove 방향전환");
			CellArrived = false;
			MapManager.Inst.SetMonsterCollision(CellPos.x, CellPos.y, false);
			MapManager.Inst.SetMonsterCollision(_cellXPos, _cellYPos, true);
			Debug.Log($"{CellPos.x}, {CellPos.y} -> {_cellXPos}, {_cellYPos}");
		}
		

		m_dest.Enqueue(dest);

		//m_eState = eState.Chase;
	}


	bool CanGo(Vector2Int _from, Vector2Int _to)
	{
		if (Math.Abs(_from.x - _to.x) <= 1 && Math.Abs(_from.y - _to.y) <= 1) return true;
		return false;
	}

	public void Hit(Skill _skill)
	{
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

	public void RemoveTarget(PlayerController _target)
	{
		if (m_targets.Count == 0) return;

		m_targets.Remove(_target);
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

		m_targets.Add(pc);
	}

	// 몬스터가 셀과 셀 사이에 있을 때 플레이어가 exit하면 거기서 멈추게 하지 말것
	void OnTriggerExit2D(Collider2D _other)
	{
		if (_other.gameObject.tag != "Player") return;

		foreach(PlayerController pc in m_targets)
		{
			if(pc.name == _other.gameObject.name)
			{
				m_targets.Remove(pc);
				break;
			}
		}
		if (m_targets.Count == 0)
		{
			//m_path = null;
			//m_target = null;
		}
	}
}
