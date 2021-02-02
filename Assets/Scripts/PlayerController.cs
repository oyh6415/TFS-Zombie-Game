using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    public AudioClip itemPickupClip;
    public int lifeRemains = 3;
    private AudioSource playerAudioPlayer;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerHealth.OnDeath += HandleDeath;

        UIManager.Instance.UpdateLifeText(lifeRemains);

        Cursor.visible = false; //마우스 커서
    }
    
    private void HandleDeath()
    {
        playerMovement.enabled = false; //움직임
        playerShooter.enabled = false; //총

        if (lifeRemains > 0) //라이프가 남아 있는 경우
        {
            lifeRemains--; //라이프를 깎고
            UIManager.Instance.UpdateLifeText(lifeRemains);
            Invoke("Respawn", 3f); //플레이어를 리스폰
        }
        else
        {
            GameManager.Instance.EndGame();
        }
        Cursor.visible = true;
    }

    public void Respawn()
    {
        //OnEnable, OnDesable 때문
        gameObject.SetActive(false);
        //리스폰 장소
        transform.position = Utility.GetRandomPointOnNavMesh(transform.position, 30f, NavMesh.AllAreas);
        playerMovement.enabled = true;
        playerShooter.enabled = true;
        gameObject.SetActive(true);
        //총알 채우기
        playerShooter.gun.ammoRemain = 120;

        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerHealth.dead) return; //플레이어가 죽은 상태면 종료

        var item = other.GetComponent<IItem>(); //아이템 확인

        if (item != null) //아이템이면
        {
            item.Use(gameObject); //사용
            playerAudioPlayer.PlayOneShot(itemPickupClip);
        }
    }
}