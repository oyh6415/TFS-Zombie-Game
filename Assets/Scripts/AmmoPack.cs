using UnityEngine;

public class AmmoPack : MonoBehaviour, IItem
{
    public int ammo = 30; //플레이어에게 추가할 총알의 양

    public void Use(GameObject target)
    {
        var playerShooter = target.GetComponent<PlayerShooter>();

        if (playerShooter != null && playerShooter.gun != null) //총을 들고 있다면
        {
            playerShooter.gun.ammoRemain += ammo; //총알 추가
        }
        //아이템 파괴
        Destroy(gameObject);
    }
}