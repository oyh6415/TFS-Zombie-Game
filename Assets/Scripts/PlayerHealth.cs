using UnityEngine;

public class PlayerHealth : LivingEntity
{
    private Animator animator;
    private AudioSource playerAudioPlayer;

    public AudioClip deathClip;
    public AudioClip hitClip;


    private void Awake()
    {
        playerAudioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateUI();
    }
    
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateHealthText(dead ? 0f : health); //죽었다면 0, 그 외는 체력 출력
    }
    
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage)) return false;
        //피격 이펙트
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, 
            damageMessage.hitNormal, transform, EffectManager.EffectType.Flesh);

        UpdateUI();

        return true;
    }
    
    public override void Die()
    {
        base.Die();
        playerAudioPlayer.PlayOneShot(deathClip);
        animator.SetTrigger("Die");

        UpdateUI();
    }
}