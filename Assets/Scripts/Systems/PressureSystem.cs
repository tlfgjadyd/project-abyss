using UnityEngine;

public class PressureSystem : MonoBehaviour
{
    public static PressureSystem Instance { get; private set; }

    // ── 기본값 (StageData가 없을 때 fallback / 테스트 모드 인스펙터 값) ──
    [Header("Pressure Zone (fallback / test)")]
    [Tooltip("이 Y좌표 아래부터 압력 시작")]
    [SerializeField] private float pressureStartY = -5f;
    [Tooltip("이 Y좌표에서 압력 최대")]
    [SerializeField] private float pressureMaxY = -30f;

    [Header("Penalty / Bonus (fallback / test)")]
    [Tooltip("최대 압력 시 이동속도 감소 비율")]
    [SerializeField] private float maxMoveSpeedPenalty = 0.3f;
    [Tooltip("최대 압력 시 공격속도 감소 비율")]
    [SerializeField] private float maxAttackSpeedPenalty = 0.3f;
    [Tooltip("최대 압력 시 경험치 보너스 비율")]
    [SerializeField] private float maxExpBonus = 0.5f;

    [Header("Activation")]
    [Tooltip("StageData가 없을 때 사용. StageData.pressureEnabled가 있으면 그 값으로 덮어씌워짐.")]
    [SerializeField] private bool isActive = false;

    private PlayerStats stats;

    /// <summary>저항이 반영된 현재 압력 (0 = 없음 ~ 1 = 최대). 페널티 계산 및 UI 표시에 사용.</summary>
    public float CurrentPressure { get; private set; }

    /// <summary>저항이 적용되지 않은 원시 압력. 환경 자체의 압력으로, 경험치 보너스는 이 값을 기준으로 한다.</summary>
    public float RawPressure { get; private set; }

    /// <summary>경험치 획득 배율 (1.0 + 압력 보너스). 저항과 무관하게 환경 보상으로 동작.</summary>
    public float ExpMultiplier => 1f + RawPressure * maxExpBonus;

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

        // StageManager가 있고 현재 스테이지 데이터가 있으면, 그 값에 따라 자동 구성.
        // (없으면 인스펙터의 isActive 및 인스펙터 파라미터 그대로 유지 → 수동 테스트 가능)
        if (StageManager.Instance != null && StageManager.Instance.CurrentStage != null)
        {
            ConfigureFromStage(StageManager.Instance.CurrentStage);
        }
    }

    /// <summary>
    /// 스테이지 데이터로부터 압력 파라미터를 구성하고 활성/비활성 처리.
    /// 씬 시작 시 또는 스테이지 전환 시 호출.
    /// </summary>
    public void ConfigureFromStage(StageData stage)
    {
        if (stage == null) return;

        pressureStartY        = stage.pressureStartY;
        pressureMaxY          = stage.pressureMaxY;
        maxMoveSpeedPenalty   = stage.pressureMovePenalty;
        maxAttackSpeedPenalty = stage.pressureAttackPenalty;
        maxExpBonus           = stage.pressureExpBonus;

        SetActive(stage.pressureEnabled);
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

        // 압력 저항은 페널티에만 적용 (경험치 보너스는 raw 기준 유지)
        float effective = raw * (1f - Mathf.Clamp01(stats.pressureResistance));

        bool rawChanged = !Mathf.Approximately(RawPressure, raw);
        bool effChanged = !Mathf.Approximately(CurrentPressure, effective);
        if (!rawChanged && !effChanged) return;

        RawPressure = raw;

        if (effChanged)
        {
            CurrentPressure = effective;
            ApplyToStats(effective);
            OnPressureChanged?.Invoke(CurrentPressure);
        }
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
            RawPressure     = 0f;
            CurrentPressure = 0f;
            ApplyToStats(0f);
            OnPressureChanged?.Invoke(0f);
        }
    }
}
