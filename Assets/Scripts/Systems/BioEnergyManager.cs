using UnityEngine;

public class BioEnergyManager : MonoBehaviour
{
    public static BioEnergyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float maxEnergy = 200f;

    [Header("Debug")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private float testConsumeAmount = 30f;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => maxEnergy;

    // current, max
    public System.Action<float, float> OnEnergyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        // 테스트 키 (Tab) — 에너지 소비 확인용
        if (testMode && Input.GetKeyDown(KeyCode.Tab))
            ConsumeEnergy(testConsumeAmount);
    }

    // ── 외부 호출 ────────────────────────────────

    public void AddEnergy(float amount)
    {
        CurrentEnergy = Mathf.Min(CurrentEnergy + amount, maxEnergy);
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
