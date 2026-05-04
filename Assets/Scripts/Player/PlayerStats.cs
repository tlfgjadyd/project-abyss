using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float attackPower = 10f;
    public float attackSpeed = 1f;      // 공격 쿨타임 배율 (1 = 기본)
    public float hpRegenPerSecond = 0f; // 세포 재생 패시브로 증가

    // 압력 시스템
    [HideInInspector] public float pressureResistance     = 0f;  // 0 ~ 0.5  (메타 업그레이드)
    [HideInInspector] public float pressureMoveMultiplier   = 1f;  // PressureSystem이 설정
    [HideInInspector] public float pressureAttackMultiplier = 1f;

    /// <summary>압력 페널티가 반영된 실제 이동속도</summary>
    public float EffectiveMoveSpeed   => moveSpeed   * pressureMoveMultiplier;
    /// <summary>압력 페널티가 반영된 실제 공격속도</summary>
    public float EffectiveAttackSpeed => attackSpeed * pressureAttackMultiplier;

    // 이벤트
    public System.Action<float, float> OnHpChanged;  // current, max
    public System.Action OnDeath;

    void Awake()
    {
        currentHp = maxHp;
    }

    void Update()
    {
        if (hpRegenPerSecond > 0f &&
            GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Heal(hpRegenPerSecond * Time.deltaTime);
        }
    }

    // 돌진 무적 등 외부에서 설정
    public bool IsInvincible { get; set; }

    public void TakeDamage(float amount)
    {
        if (currentHp <= 0f) return;
        if (IsInvincible) return;

        currentHp = Mathf.Clamp(currentHp - amount, 0f, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (currentHp >= maxHp) return;

        currentHp = Mathf.Clamp(currentHp + amount, 0f, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    void Die()
    {
        OnDeath?.Invoke();
        GameManager.Instance.TriggerGameOver();
    }
}
