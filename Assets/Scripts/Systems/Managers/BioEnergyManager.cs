using UnityEngine;

public class BioEnergyManager : MonoBehaviour
{
    public static BioEnergyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float maxEnergy = 200f;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => maxEnergy;

    /// <summary>감각 붕괴 돌연변이 등에서 외부 설정. 적 처치 시 받는 에너지에 곱셈으로 적용. 기본 1.0</summary>
    public float ChargeRateMultiplier { get; set; } = 1f;

    // current, max
    public System.Action<float, float> OnEnergyChanged;
    public System.Action OnEnergyInsufficient;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── 외부 호출 ────────────────────────────────

    public void AddEnergy(float amount)
    {
        CurrentEnergy = Mathf.Min(CurrentEnergy + amount * ChargeRateMultiplier, maxEnergy);
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    /// <summary>에너지 소모. 성공 시 true, 부족 시 false 반환.</summary>
    public bool ConsumeEnergy(float amount)
    {
        if (!CanConsume(amount)) return false;

        CurrentEnergy -= amount;
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        return true;
    }

    public bool CanConsume(float amount) => CurrentEnergy >= amount;
}
