using UnityEngine;

/// <summary>
/// 산갈치 보스 마디 — 자식 GameObject에 부착. 받는 데미지를 부모 BossBase로 전달.
/// 마디별 가시성/위치는 자식 SpriteRenderer로 표현.
/// 마디는 부모 BossBase의 Collider2D 외에 별개 trigger Collider2D를 보유하여 hitbox 확장.
/// </summary>
public class OarfishSegment : MonoBehaviour, IDamageable
{
    [Tooltip("받은 데미지 배율 (1=동일). 머리/꼬리 차등 가능. 기본 1.0")]
    [SerializeField] private float damageMultiplier = 1f;

    [Tooltip("부모 BossBase. 미할당 시 부모에서 자동 탐색.")]
    [SerializeField] private BossBase parentBoss;

    void Awake()
    {
        if (parentBoss == null)
            parentBoss = GetComponentInParent<BossBase>();
    }

    public void TakeDamage(float amount)
    {
        if (parentBoss == null) return;
        parentBoss.TakeDamage(amount * damageMultiplier);
    }
}
