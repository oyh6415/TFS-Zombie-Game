using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        Ready,
        Empty,
        Reloading
    }
    public State state { get; private set; }
    
    private PlayerShooter gunHolder;
    private LineRenderer bulletLineRenderer; //총알 궤적 그림
    
    private AudioSource gunAudioPlayer;
    public AudioClip shotClip;
    public AudioClip reloadClip;
    
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;
    
    public Transform fireTransform; //총알이 나가는 방향, 위치
    public Transform leftHandMount; //왼손의 위치

    public float damage = 25;
    public float fireDistance = 100f;

    public int ammoRemain = 100; //남은 탄약 수
    public int magAmmo; //현재 탄창의 탄약 수
    public int magCapacity = 30; //탄창 용량

    public float timeBetFire = 0.12f; //총알 발사 사이의 간격
    public float reloadTime = 1.8f; //재장전에 걸리는 시간
    
    [Range(0f, 10f)] public float maxSpread = 3f; //발사한 총알이 흝어지는 범위
    [Range(1f, 10f)] public float stability = 1f; //반동이 증가하는 속도
    [Range(0.01f, 3f)] public float restoreFromRecoilSpeed = 2f; //연사를 종료한 뒤 탄 퍼짐의 속도가 0로 돌아오는 시간
    private float currentSpread;
    private float currentSpreadVelocity;

    private float lastFireTime; //가장 최근에 발사한 시간

    private LayerMask excludeTarget; //총 쏘면 안되는 레이어

    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2; //사용할 점의 개수
        bulletLineRenderer.enabled = false;
    }

    public void Setup(PlayerShooter gunHolder)
    {
        this.gunHolder = gunHolder;
        excludeTarget = gunHolder.excludeTarget;
    }

    private void OnEnable()
    {
        magAmmo = magCapacity; //탄창의 탄약 수 30
        currentSpread = 0f; //탄 퍼짐 0
        lastFireTime = 0f; //최근 발사 시간 0
        state = State.Ready;
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //스크립트 내의 코루틴 모두 비활성화
    }

    public bool Fire(Vector3 aimTarget)
    {
        if (state == State.Ready &&Time.time>=lastFireTime+timeBetFire)
        { //준비가 됐고 현재 시간이 발사 했던 시간 + 발사 하는 시간보다 길면 발사 가능
            var fireDirection = aimTarget - fireTransform.position; //조준하는 곳 - 발사 지점 = 가는 거리, 방향

            var xError = Utility.GedRandomNormalDistribution(0f, currentSpread); //currentSpread가 0에 가까우면 mean에 가까움
            var yError = Utility.GedRandomNormalDistribution(0f, currentSpread);

            fireDirection = Quaternion.AngleAxis(yError,Vector3.up)*fireDirection; //y축으로 회전
            fireDirection = Quaternion.AngleAxis(xError, Vector3.right) * fireDirection; //x축으로 회전

            currentSpread += 1f / stability; //쏠수록 반동 증가

            lastFireTime = Time.time;
            Shot(fireTransform.position, fireDirection);

            return true;
        }
        return false;
    }
    
    private void Shot(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit hit;
        Vector3 hitPosition;

        if(Physics.Raycast(startPoint,direction,out hit, fireDistance, ~excludeTarget))
        {
            var target = hit.collider.GetComponent<IDamageable>(); //데미지를 받을 수 있는 타입인지 확인

            if (target != null)
            {
                DamageMessage damageMessage;

                damageMessage.damager = gunHolder.gameObject; //데미지를 주는 객체
                damageMessage.amount = damage; //데미지 값
                damageMessage.hitPoint = hit.point; //타격이 가한 위치
                damageMessage.hitNormal = hit.normal; //충돌한 지점

                target.ApplyDamage(damageMessage);
            }
            hitPosition = hit.point;
        }
        else //총에 아무도 맞지 않을 경우
        {
            hitPosition = startPoint + direction * fireDistance; //시작지점 + 방향*총알 사정 거리
        }

        StartCoroutine(ShotEffect(hitPosition));

        magAmmo--;
        if (magAmmo <= 0) state = State.Empty;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play(); //파티클 재생
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip); //소리 재생

        bulletLineRenderer.enabled = true;
        bulletLineRenderer.SetPosition(0, fireTransform.position); //선 그리기
        bulletLineRenderer.SetPosition(1, hitPosition);

        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false; //대기 시간으로 번쩍 하는 효과
    }
    
    public bool Reload()
    {
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity) //재장전 중, 총 탄약이 없을 때, 탄창이 가득 차 있을 때
        {
            return false;
        }
        //위 해당사항 없으면 재장전
        StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        state = State.Reloading;
        gunAudioPlayer.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo, 0, ammoRemain); //0과 ammoRemain 사이의 값으로 자름

        magAmmo += ammoToFill; //넣어야 하는만큼 넣기
        ammoRemain -= ammoToFill; //총 탄약 수에서 빼기

        state = State.Ready;
    }

    private void Update()
    {
        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread); //maxSpread 값 이상으로 값 증가X
        currentSpread //탄 퍼짐이 0으로 돌아옴
            = Mathf.SmoothDamp(currentSpread, 0f,ref currentSpreadVelocity, 1f / restoreFromRecoilSpeed);
    }
}