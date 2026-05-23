using UnityEngine;

/// <summary>
/// 자기 유도 — ExpOrb의 흡인 범위 배율을 증가시키는 패시브.
/// 자체 로직 없이 PlayerStats.magneticRangeMultiplier만 조정.
/// SkillEffectApplier가 레벨업 시 stats.magneticRangeMultiplier += delta로 누적.
/// </summary>
public class MagneticInduction : MonoBehaviour
{
    // 마커 컴포넌트 — 로직은 PlayerStats.magneticRangeMultiplier가 처리
    // SkillEffectApplier가 이 컴포넌트의 enabled 여부로 활성화 추적
}
