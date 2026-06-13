using UnityEngine;

/// <summary>
/// 손맛 공통 helper — 카피 스킬 / 강한 충돌 시 hitstop + 카메라 미세 흔들림 일괄 호출.
/// 사용: HitEffectGlobal.PunchSmall();  // 일반 공격
///       HitEffectGlobal.PunchMedium(); // 카피 스킬 충돌
///       HitEffectGlobal.PunchBig();    // 보스 페이즈 전환
/// </summary>
public static class HitEffectGlobal
{
    public static void PunchSmall()
    {
        HitStop.Instance?.Freeze(0.03f);
        CameraEffect.Instance?.Shake(0.08f, 0.10f);
    }

    public static void PunchMedium()
    {
        HitStop.Instance?.Freeze(0.06f);
        CameraEffect.Instance?.Shake(0.18f, 0.18f);
    }

    public static void PunchBig()
    {
        HitStop.Instance?.Freeze(0.12f);
        CameraEffect.Instance?.Shake(0.35f, 0.35f);
    }
}
