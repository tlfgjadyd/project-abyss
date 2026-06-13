using UnityEngine;

/// <summary>
/// 자기 유도 — ExpOrb 흡인 범위 배율 증가 패시브.
/// 평소 로직은 PlayerStats.magneticRangeMultiplier가 처리(SkillEffectApplier가 레벨업 시 누적).
/// Lv4: 일정 주기마다 맵 전체의 경험치를 끌어오는 효과 추가(EnableVacuum로 활성).
/// </summary>
public class MagneticInduction : MonoBehaviour
{
    [Header("Lv4 — 주기적 전체 흡인")]
    [Tooltip("전체 흡인 발동 주기(초)")]
    [SerializeField] private float vacuumInterval = 90f;
    [Tooltip("전체 흡인 지속(초) — 이 동안 모든 ExpOrb가 거리 무시하고 끌려옴")]
    [SerializeField] private float vacuumDuration = 2f;

    private bool vacuumEnabled;
    private float vacuumTimer;

    /// <summary>Lv4 도달 시 SkillEffectApplier가 호출.</summary>
    public void EnableVacuum()
    {
        vacuumEnabled = true;
        vacuumTimer = vacuumInterval;
    }

    void Update()
    {
        if (!vacuumEnabled) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        vacuumTimer -= Time.deltaTime;
        if (vacuumTimer <= 0f)
        {
            vacuumTimer = vacuumInterval;
            ExpOrb.GlobalVacuumUntil = Time.time + vacuumDuration;
        }
    }
}
