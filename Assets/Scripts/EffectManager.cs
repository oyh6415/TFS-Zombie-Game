using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private static EffectManager m_Instance;
    public static EffectManager Instance //싱글톤
    {
        get
        {
            if (m_Instance == null) m_Instance = FindObjectOfType<EffectManager>();
            return m_Instance;
        }
    }

    public enum EffectType
    {
        Common,
        Flesh
    }
    
    public ParticleSystem commonHitEffectPrefab;
    public ParticleSystem fleshHitEffectPrefab; //생명체가 총을 맞았을 때 파티클
    
    public void PlayHitEffect(Vector3 pos, Vector3 normal, Transform parent = null, EffectType effectType = EffectType.Common)
    {
        var targetPrefab = commonHitEffectPrefab;

        if (effectType == EffectType.Flesh)
        {
            targetPrefab = fleshHitEffectPrefab;
        }
        //총알이 충돌한 곳에 파티클 생성
        var effect = Instantiate(targetPrefab, pos, Quaternion.LookRotation(normal));
        if (parent != null) effect.transform.SetParent(parent);

        effect.Play();
    }
}