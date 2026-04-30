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

    // 압력 시스템용 (3주차에 연결)
    [HideInInspector] public float pressureResistance = 0f; // 0 ~ 0.5

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

    public void TakeDamage(float amount)
    {
        if (currentHp <= 0f) return;

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
