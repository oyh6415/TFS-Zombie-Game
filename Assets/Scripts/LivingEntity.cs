using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 100f;
    public float health { get; protected set; } 
    public bool dead { get; protected set; }
    
    public event Action OnDeath;
    
    private const float minTimeBetDamaged = 0.1f; //공격과 공격 사이의 최소 대기 시간
    private float lastDamagedTime; //최근에 공격당한 시간

    protected bool IsInvulnerabe //공격당한 후 대기 시간 지나면 flase, 지나지 않으면 true로 무적 모드
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }
    
    protected virtual void OnEnable()
    {
        dead = false;
        health = startingHealth;
    }

    public virtual bool ApplyDamage(DamageMessage damageMessage) //IDamageable
    {
        //무적 상태, 예외로 자기 자신에게 공격했을 때, 죽었을 때는 공격 무효
        if (IsInvulnerabe || damageMessage.damager == gameObject || dead) return false;

        lastDamagedTime = Time.time;
        health -= damageMessage.amount;
        
        if (health <= 0) Die();

        return true;
    }
    
    public virtual void RestoreHealth(float newHealth) //체력 회복
    {
        if (dead) return;
        
        health += newHealth;
    }
    
    public virtual void Die()
    {
        if (OnDeath != null) OnDeath();
        
        dead = true;
    }
}