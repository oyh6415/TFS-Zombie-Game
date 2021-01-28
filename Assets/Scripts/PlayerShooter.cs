using UnityEditorInternal;
using UnityEngine;


public class PlayerShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle,
        HipFire
    }

    public AimState aimState { get; private set; }

    public Gun gun;
    public LayerMask excludeTarget;
    
    private PlayerInput playerInput;
    private Animator playerAnimator;
    private Camera playerCamera;

    private float waitingTimeForReleasingAim = 2.5f; //HipFire 상태에서 발사가 없으면 Idle로 상태가 돌아오는 시간
    private float lastFireInputTime;

    private Vector3 aimPoint; //조준한 대상
    //카메라의 y축과 플레이어의 y축이 많이 차이 나는지
    private bool linedUp => !(Mathf.Abs( playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
    //정면에 총 발사할 공간이 있는지
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y,gun.fireTransform.position, ~excludeTarget);
    
    void Awake()
    {
        //비트 처리
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer))) //excludeTarget에 플렝이어 레이어가 없다면
        {
            excludeTarget |= 1 << gameObject.layer; //플레이어 레이어를 추가
        }
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true);
        gun.Setup(this);
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (playerInput.fire) //fire 버튼을 입력하면 true
        {
            lastFireInputTime = Time.time; //발사 버튼 누르는 시간 갱신
            Shoot();
        }
        else if (playerInput.reload) //reload 버튼을 입력하면 true
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        //상체 앵글 조정
        var angle = playerCamera.transform.eulerAngles.x; 
        if (angle > 270f) angle -= 360f;

        angle = angle / -180f + 0.5f;
        playerAnimator.SetFloat("Angle", angle);

        if (!playerInput.fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }

        UpdateUI();
    }

    public void Shoot()
    {
        if (aimState == AimState.Idle)
        {
            if (linedUp) aimState = AimState.HipFire;
        }
        else if (aimState == AimState.HipFire)
        {
            if (hasEnoughDistance) //총을 쏠 거리가 있다면
            {
                if (gun.Fire(aimPoint)) 
                {
                    playerAnimator.SetTrigger("Shoot");
                }
            }
            else //없다면
            {
                aimState = AimState.Idle;
            }
        }
    }

    public void Reload()
    {
        if (gun.Reload())
        {
            playerAnimator.SetTrigger("Reload");
        }
    }

    private void UpdateAimTarget() //Aim 수정
    {
        RaycastHit hit;

        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); //화면의 정중앙

        if(Physics.Raycast(ray,out hit, gun.fireDistance, ~excludeTarget)) //화면의 정중앙에서 쐈을 때 맞는 게 있다면
        {
            aimPoint = hit.point; //조준 위치 지정

            if(Physics.Linecast(gun.fireTransform.position,hit.point,out hit, ~excludeTarget)) //발사 부분에서 목표 지점까지 맞는 게 있다면
            {
                aimPoint = hit.point;
            }
        }
        else //맞는 게 없다면
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    private void UpdateUI() //UI 실행
    {
        //총이 없거나 UIManager가 없으면 종료
        if (gun == null || UIManager.Instance == null) return;
        
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain); //탄약 UI
        
        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance); //조준점
        UIManager.Instance.UpdateCrossHairPosition(aimPoint); //조준하는 위치 업데이트
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //총이 없거나 장전 중이면 왼손 떼기
        if (gun == null || gun.state == Gun.State.Reloading) return;

        //왼손 총에 붙여 두기
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
    }
}