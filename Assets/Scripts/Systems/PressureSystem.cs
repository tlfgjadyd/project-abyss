using UnityEngine;

public class PressureSystem : MonoBehaviour
{
    public static PressureSystem Instance { get; private set; }

    [Header("Pressure Zone")]
    [Tooltip("이 Y좌표 아래부터 압력 시작")]
    [SerializeField] private float pressureStartY = -5f;
    [Tooltip("이 Y좌표에서 압력 최대")]
    [SerializeField] private float pressureMaxY = -30f;

    [Header("Penalty / Bonus")]
    [Tooltip("최대 압력 시 이동속도 감소 비율")]
    [SerializeField] private float maxMoveSpeedPenalty = 0.3f;
    [Tooltip("최대 압력 시 공격속도 감소 비율")]
    [SerializeField] private float maxAttackSpeedPenalty = 0.3f;
    [Tooltip("최대 압력 시 경험치 보너스 비율")]
    [SerializeField] private float maxExpBonus = 0.5f;

    [Header("Activation")]
    [Tooltip("2~3스테이지 진입 시 true로 설정")]
    [SerializeField] private bool isActive = false;

    private PlayerStats stats;

    /// <summary>현재 유효 압력 (0 = 없음 ~ 1 = 최대)</summary>
    public float CurrentPressure { get; private set; }

    /// <summary>경험치 획득 배율 (1.0 + 압력 보너스)</summary>
    public float ExpMultiplier => 1f + CurrentPressure * maxExpBonus;

    public System.Action<float> OnPressureChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            stats = player.GetComponent<PlayerStats>();

        ApplyToStats(0f);
    }

    void Update()
    {
        if (!isActive || stats == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        UpdatePressure();
    }

    void UpdatePressure()
    {
        float playerY = stats.transform.position.y;

        // Y가 낮을수록(아래로 내려갈수록) 압력 증가
        float raw = Mathf.Clamp01(
            Mathf.InverseLerp(pressureStartY, pressureMaxY, playerY)
        );

        // 압력 저항 반영
        float effective = raw * (1f - Mathf.Clamp01(stats.pressureResistance));

        if (Mathf.Approximately(CurrentPressure, effective)) return;

        CurrentPressure = effective;
        ApplyToStats(effective);
        OnPressureChanged?.Invoke(CurrentPressure);
    }

    void ApplyToStats(float pressure)
    {
        if (stats == null) return;
        stats.pressureMoveMultiplier   = 1f - pressure * maxMoveSpeedPenalty;
        stats.pressureAttackMultiplier = 1f - pressure * maxAttackSpeedPenalty;
    }

    /// <summary>스테이지 전환 시 외부에서 호출</summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            CurrentPressure = 0f;
            ApplyToStats(0f);
            OnPressureChanged?.Invoke(0f);
        }
    }
}
