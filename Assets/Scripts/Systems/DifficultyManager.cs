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

    public float ElapsedTime { get; private set; }

    /// <summary>0(시작) ~ 1(절정) 진행도</summary>
    public float Progress => Mathf.Clamp01(ElapsedTime / Mathf.Max(peakTime, 0.001f));

    public float CurrentHpScale          => Mathf.Lerp(1f, hpMultiplierPeak,     Progress);
    public float CurrentDamageScale      => Mathf.Lerp(1f, damageMultiplierPeak, Progress);
    public float CurrentIntervalScale    => Mathf.Lerp(1f, intervalMultiplierPeak,  Progress);
    public float CurrentMaxActiveScale   => Mathf.Lerp(1f, maxActiveMultiplierPeak, Progress);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        ElapsedTime += Time.deltaTime;
    }

    /// <summary>스테이지 전환 등에서 호출하여 누적 시간 리셋.</summary>
    public void ResetElapsed()
    {
        ElapsedTime = 0f;
    }
}
