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
    
    //[HideInInspector] 
    public LivingEntity targetEntity;
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
        float runSpeed, float patrolSpeed, Color skinColor)
    {

    }

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (dead) return;
    }

    private IEnumerator UpdatePath()
    {
        while (!dead)
        {
            if (hasTarget)
            {
                agent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                if (targetEntity != null) targetEntity = null;
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage)) return false;
        
        return true;
    }

    public void BeginAttack()
    {
        state = State.AttackBegin;

        agent.isStopped = true;
        animator.SetTrigger("Attack");
    }

    public void EnableAttack()
    {
        state = State.Attacking;
        
        lastAttackedTargets.Clear();
    }

    public void DisableAttack()
    {
        state = State.Tracking;
        
        agent.isStopped = false;
    }

    private bool IsTargetOnSight(Transform target)
    {
        return false;
    }
    
    public override void Die()
    {

    }
}