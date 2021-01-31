using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //네이게이션 시스템

#if UNITY_EDITOR //유니티 에디터 코드를 별개로 묶음. 빌드할 때 사라짐
using UnityEditor;
#endif

public class Enemy : LivingEntity
{
    private enum State
    {
        Patrol,
        Tracking,
        AttackBegin,
        Attacking
    }
    
    private State state;
    
    private NavMeshAgent agent;
    private Animator animator;

    public Transform attackRoot; //공격하는 위치
    public Transform eyeTransform; //감지할 시야 기준
    
    private AudioSource audioPlayer;
    public AudioClip hitClip;
    public AudioClip deathClip;
    
    private Renderer skinRenderer;

    public float runSpeed = 10f; //좀비 이동 속도
    [Range(0.01f, 2f)] public float turnSmoothTime = 0.1f; //방향 회전할 때의 지연 시간
    private float turnSmoothVelocity;
    
    public float damage = 30f;
    public float attackRadius = 2f; //공격 반경
    private float attackDistance;
    
    public float fieldOfView = 50f; //시야를 얼마나 넓게 보는지
    public float viewDistance = 10f; //시야의 거리
    public float patrolSpeed = 3f; //공격, 추적하지 않을 때의 속도
    
    [HideInInspector] public LivingEntity targetEntity;
    public LayerMask whatIsTarget;

    private RaycastHit[] hits = new RaycastHit[10];
    private List<LivingEntity> lastAttackedTargets = new List<LivingEntity>();
    
    //추적하는 대상이 존재하고 죽지 않았을 때 true
    private bool hasTarget => targetEntity != null && !targetEntity.dead;
    

#if UNITY_EDITOR

    private void OnDrawGizmosSelected() //오브젝트가 선택됐을 때만 보임
    {
        if (attackRoot != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(attackRoot.position, attackRadius);
        }

        if (eyeTransform != null) //시야가 있다면
        {
            var leftEyeRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up); //시야의 왼쪽 부분
            var leftRayDirection = leftEyeRotation * transform.forward;
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            //호를 그림
            Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, viewDistance);
        }
    }
    
#endif
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();
        skinRenderer = GetComponentInChildren<Renderer>();

        var attackPivot = attackRoot.position;
        attackPivot.y = transform.position.y;
        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius; //공격범위

        agent.stoppingDistance = attackDistance;
        agent.speed = patrolSpeed;
    }

    public void Setup(float health, float damage,
        float runSpeed, float patrolSpeed, Color skinColor) //초기 정보
    {
        this.startingHealth = health;
        this.health = health;

        this.damage = damage;
        this.runSpeed = runSpeed;
        this.patrolSpeed = patrolSpeed;

        skinRenderer.material.color = skinColor;

        agent.speed = patrolSpeed;
    }

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }

        if (state == State.Tracking) //추적 상태라면
        {
            var distance = Vector3.Distance(targetEntity.transform.position, transform.position);

            if (distance <= attackDistance) //공격 범위 안에 있을 때
            {
                BeginAttack(); //공격
            }
        }

        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (dead) return;

        if (state == State.AttackBegin || state == State.Attacking) //공격 중일 때
        {
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position); //적에게 향하는 각도
            var targetAngleY = lookRotation.eulerAngles.y;
            //천천히 각도 변경
            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;
        }

        if (state == State.Attacking) 
        {
            var direction = transform.forward;
            var deltaDistance = agent.velocity.magnitude * Time.deltaTime;

            //공격 중인 적 감지
            var size = Physics.SphereCastNonAlloc(attackRoot.position, attackRadius, direction, hits, deltaDistance, whatIsTarget);

            for (var i = 0; i < size; i++)
            {
                var attackTargetEntity = hits[i].collider.GetComponent<LivingEntity>();
                //살아있는 생명체라면
                if (attackTargetEntity != null && !lastAttackedTargets.Contains(attackTargetEntity))
                {
                    var message = new DamageMessage();
                    message.amount = damage; 
                    message.damager = gameObject;

                    if (hits[i].distance <= 0f) //point가 0일 때를 대비
                    {
                        message.hitPoint = attackRoot.position;
                    }
                    else
                    {
                        message.hitPoint = hits[i].point;
                    }

                    message.hitNormal = hits[i].normal;

                    attackTargetEntity.ApplyDamage(message);
                    lastAttackedTargets.Add(attackTargetEntity);
                    break;
                }
            }

        }
    }

    private IEnumerator UpdatePath()
    {
        while (!dead) //사망하지 않으면 계속 실행
        {
            if (hasTarget) //목표 타겟이 있을 때
            {
                if (state == State.Patrol) //정찰 종료
                {
                    state = State.Tracking;
                    agent.speed = runSpeed;
                }
                agent.SetDestination(targetEntity.transform.position); //타겟 위치로 이동
            }
            else //타겟이 없을 때
            {
                if (targetEntity != null) targetEntity = null;

                if (state != State.Patrol) //정찰 시작
                {
                    state = State.Patrol;
                    agent.speed = patrolSpeed;
                }

                if (agent.remainingDistance <= 1f) //목표 지점까지 1f 안 남았을 때
                {
                    //랜덤한 곳으로 가서 정찰
                    var patrolTargetPosition = Utility.GetRandomPointOnNavMesh(transform.position, 20f, NavMesh.AllAreas);
                    agent.SetDestination(patrolTargetPosition);
                }
                //눈을 기준으로 원을 그린 뒤 안의 방해물 감지
                var colliders = Physics.OverlapSphere(eyeTransform.position, viewDistance, whatIsTarget);

                foreach(var collider in colliders) //방해물이 살아 있는지 감지
                {
                    if (!IsTargetOnSight(collider.transform)) //타겟이 아니라면 다음 collider 확인
                    {
                        continue;
                    }

                    var livingEntity = collider.GetComponent<LivingEntity>(); 

                    if (livingEntity != null && !livingEntity.dead) //살아있는 생명체 감지했다면
                    {
                        targetEntity = livingEntity; //타겟 설정
                        break; //foreach 종료
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage)) return false; //부모 함수가 false 일 때

        if (targetEntity == null) //타겟이 없을 때 공격 받으면
        {
            targetEntity = damageMessage.damager.GetComponent<LivingEntity>(); //공격한 객체를 타겟으로 인식
        }

        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, transform, EffectManager.EffectType.Flesh);
        audioPlayer.PlayOneShot(hitClip);

        return true;
    }

    public void BeginAttack()
    {
        state = State.AttackBegin;

        agent.isStopped = true; //추적 중단
        animator.SetTrigger("Attack");
    }

    public void EnableAttack()
    {
        state = State.Attacking;
        
        lastAttackedTargets.Clear();
    }

    public void DisableAttack()
    {
        if (hasTarget)
        {
            state = State.Tracking;
        }
        else
        {
            state = State.Patrol;
        }
        
        agent.isStopped = false;
    }

    private bool IsTargetOnSight(Transform target)
    {
        var direction = target.position - eyeTransform.position;
        direction.y = eyeTransform.forward.y;

        if (Vector3.Angle(direction, eyeTransform.forward) > fieldOfView * 0.5f) //시야 안에 없다면
        {
            return false;
        }
        direction= target.position - eyeTransform.position;
        RaycastHit hit;
        //시야 안에 있을 때
        if (Physics.Raycast(eyeTransform.position,direction,out hit, viewDistance, whatIsTarget))
        {
            if (hit.transform == target) //확인할 생명체가 맞다면
            {
                return true;
            }
        }
        return false;
    }
    
    public override void Die()
    {
        base.Die(); //부모 함수 실행

        GetComponent<Collider>().enabled = false; //물리 제거
        agent.enabled = false; //네이게이션 off
        animator.applyRootMotion = true; //애니메이션을 통한 위치 이동 on
        animator.SetTrigger("Die");
        audioPlayer.PlayOneShot(deathClip);
    }
}