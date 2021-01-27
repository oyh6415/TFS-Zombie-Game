using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput playerInput;
    private Animator animator;
    
    private Camera followCam;
    
    public float speed = 6f; //움직임 속도
    public float jumpVelocity = 20f; //점프 속도
    [Range(0.01f, 1f)] public float airControlPercent; //공중에 체류하는 동안 몇 퍼센트 확률로 통제하는지

    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;
    
    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    
    private float currentVelocityY; //중력이 없기 때문에
    
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude; //벡터의 길기(대각선)
    
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        followCam = Camera.main;
    }

    private void FixedUpdate()
    {
        if (currentSpeed > 0.2f || playerInput.fire) Rotate(); //플레이어가 속도를 내거나 총을 쏘면 카메라 회전

        Move(playerInput.moveInput);
        
        if (playerInput.jump) Jump();
    }

    private void Update()
    {
        UpdateAnimation(playerInput.moveInput);
    }

    public void Move(Vector2 moveInput)
    {
        var targetSpeed = speed * moveInput.magnitude;
        var moveDirection =Vector3.Normalize
            (transform.forward * moveInput.y + transform.right * moveInput.x);

        var smoothTime = characterController.isGrounded ? speedSmoothTime : speedSmoothTime / airControlPercent;
        // 땅에 닿지 않았다면 느리게 조작

        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, smoothTime); //지연

        currentVelocityY += Time.deltaTime * Physics.gravity.y; //중력가속도만큼 y축에 더함

        var velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY; //최종값

        characterController.Move(velocity * Time.deltaTime); //캐릭터 이동

        if (characterController.isGrounded) currentVelocityY = 0f;
    }

    public void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

        transform.eulerAngles = Vector3.up * targetRotation;
    }

    public void Jump()
    {
        if (!characterController.isGrounded) return; //공중에 있을 때 점프X
        currentVelocityY = jumpVelocity;
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        //현재 속도가 0이라면 걷는 애니메이션 작동X
        var animationSpeedPercent = currentSpeed / speed;
        animator.SetFloat("Vertical Move", moveInput.y* animationSpeedPercent,0.05f,Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x* animationSpeedPercent, 0.05f, Time.deltaTime);
    }
}