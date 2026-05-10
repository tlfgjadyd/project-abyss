/// <summary>
/// 플레이어가 스킬을 사용할 때 발행되는 전역 이벤트.
/// 일반 스킬(Slash, PoisonNeedle, BioticExplosion, ElectricEngine) 발동 시 호출.
/// 감각 붕괴 돌연변이 등에서 구독해 부수효과(스턴 등) 처리.
/// </summary>
public static class PlayerSkillEvents
{
    public static System.Action OnSkillUsed;
}
