using UnityEngine;

/// <summary>
/// 시간 경과에 따른 난이도 스케일링을 관리.
/// EnemySpawner는 spawn rate/maxActive 스케일을, EnemyBase는 HP/공격력 스케일을 조회.
/// peakTime에 도달하면 모든 스케일이 최대값에 도달.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Curve")]
    [Tooltip("스케일이 최대치에 도달하는 시간(초). 보스 등장 시점 또는 그 직전으로 설정 권장.")]
    [SerializeField] private float peakTime = 360f;

    [Header("Test Mode")]
    [Tooltip("체크 시 StageData를 무시하고 인스펙터의 peakTime을 사용 (단축 테스트용).")]
    [SerializeField] private bool overrideStageData = false;

    [Header("Enemy Stats at Peak")]
    [Tooltip("절정 시점의 HP 배율 (≥ 1)")]
    [SerializeField] private float hpMultiplierPeak     = 2.0f;
    [Tooltip("절정 시점의 접촉 피해 배율 (≥ 1)")]
    [SerializeField] private float damageMultiplierPeak = 1.5f;

    [Header("Spawn at Peak")]
    [Tooltip("절정 시점의 spawn interval 배율 (0 < x ≤ 1). 0.4 = 스폰 주기 60% 단축")]
    [SerializeField] private float intervalMultiplierPeak  = 0.4f;
    [Tooltip("절정 시점의 maxActiveEnemies 배율 (≥ 1)")]
    [SerializeField] private float maxActiveMultiplierPeak = 2.0f;

    [Header("Anchored Crisis Curve (Day56)")]
    [Tooltip("보스 등장 몇 초 전부터 '위기' 구간(스폰 크레셴도) 시작")]
    [SerializeField] private float crisisLeadTime = 30f;
    [Tooltip("위기 절정(보스 직전) 시 정상 스폰 강도 위에 곱해지는 배수. 1.3 = +30% 밀도")]
    [SerializeField] private float crisisSpawnBoost = 1.3f;
    [Tooltip("보스 전투 중 스폰 강도 배수 (1 미만 = 완화). 위기 절정에서 떨어뜨려 숨돌릴 틈 제공")]
    [SerializeField] private float bossActiveSpawnFactor = 0.7f;
    [Tooltip("곡선 전체 강도 전역 노브. 재밸런싱은 이 값 하나로. 1=기본")]
    [Range(0.5f, 2f)] [SerializeField] private float globalIntensityScale = 1f;

    /// <summary>위기 구간 진입(보스 임박) 시 1회 발동 — HUD가 BOSS INCOMING 경고 표시.</summary>
    public static event System.Action OnBossIncoming;

    private bool bossActive;
    private bool crisisAnnounced;

    public float ElapsedTime { get; private set; }

    /// <summary>0(시작) ~ 1(절정) 진행도</summary>
    public float Progress => Mathf.Clamp01(ElapsedTime / Mathf.Max(peakTime, 0.001f));

    /// <summary>보스 등장 시점(=peakTime, 런타임에 bossSpawnTime과 동기화).</summary>
    private float BossTime => peakTime;

    /// <summary>
    /// 정상 선형 램프 위에 곱해지는 스폰 강도 배수(앵커형 곡선).
    /// 정상=1, 위기창=1→crisisSpawnBoost 크레셴도, 보스 전투=bossActiveSpawnFactor(완화).
    /// globalIntensityScale로 전체를 한 번에 조절.
    /// </summary>
    private float SpawnPhaseFactor
    {
        get
        {
            float f;
            if (bossActive)
                f = bossActiveSpawnFactor;
            else
            {
                float crisisStart = BossTime - crisisLeadTime;
                if (crisisLeadTime > 0.01f && ElapsedTime >= crisisStart)
                {
                    float u = Mathf.Clamp01((ElapsedTime - crisisStart) / crisisLeadTime);
                    f = Mathf.Lerp(1f, crisisSpawnBoost, u);
                }
                else f = 1f;
            }
            return f * globalIntensityScale;
        }
    }

    public float CurrentHpScale          => Mathf.Lerp(1f, hpMultiplierPeak,     Progress);
    public float CurrentDamageScale      => Mathf.Lerp(1f, damageMultiplierPeak, Progress);
    // 앵커형: 정상 램프 위에 위기/완화 배수를 적용. 강도↑ = interval↓(빨라짐), maxActive↑
    public float CurrentIntervalScale    => Mathf.Lerp(1f, intervalMultiplierPeak,  Progress) / Mathf.Max(0.05f, SpawnPhaseFactor);
    public float CurrentMaxActiveScale   => Mathf.Lerp(1f, maxActiveMultiplierPeak, Progress) * SpawnPhaseFactor;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // overrideStageData=true이면 인스펙터 값 그대로 사용 (테스트 단축용)
        if (overrideStageData) return;

        // StageManager가 있으면 peakTime을 현재 스테이지의 bossSpawnTime과 동기화
        if (StageManager.Instance != null && StageManager.Instance.CurrentStage != null)
            peakTime = StageManager.Instance.CurrentStage.bossSpawnTime;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        ElapsedTime += Time.deltaTime;

        // 위기 구간 진입 시 BOSS INCOMING 경고 1회 발동 (보스 스폰보다 crisisLeadTime 먼저)
        if (!crisisAnnounced && !bossActive && BossTime > 0.01f
            && ElapsedTime >= BossTime - crisisLeadTime)
        {
            crisisAnnounced = true;
            OnBossIncoming?.Invoke();
        }
    }

    /// <summary>보스 등장/퇴장 시 BossSpawner가 호출 — 스폰 강도를 완화 모드로 전환.</summary>
    public void SetBossActive(bool active)
    {
        bossActive = active;
    }

    /// <summary>스테이지 전환 등에서 호출하여 누적 시간 리셋.</summary>
    public void ResetElapsed()
    {
        ElapsedTime = 0f;
        bossActive = false;
        crisisAnnounced = false;
    }
}
