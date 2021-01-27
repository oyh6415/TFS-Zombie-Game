using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string fireButtonName = "Fire1"; //총 발사
    public string jumpButtonName = "Jump"; //점프
    public string moveHorizontalAxisName = "Horizontal"; //좌우
    public string moveVerticalAxisName = "Vertical"; //앞뒤
    public string reloadButtonName = "Reload"; //재장전 (커스텀)


    public Vector2 moveInput { get; private set; } //private set -> 외부에서 수정할 수 없음
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool jump { get; private set; }
    
    private void Update()
    {
        if (GameManager.Instance != null //게임매니저가 존재하고
            && GameManager.Instance.isGameover) //게임오버가 됐다면
        { //유저의 입력 모두 무시
            moveInput = Vector2.zero;
            fire = false;
            reload = false;
            jump = false;
            return;
        }

        moveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
        if (moveInput.sqrMagnitude > 1) moveInput = moveInput.normalized; //대각선 값이 1을 넘으면 정규화, 1로 만듬
        // 대각선 속도가 빨라지지 않기 위함

        jump = Input.GetButtonDown(jumpButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
    }
}