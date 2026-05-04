using UnityEngine;

// 모든 카피 스킬이 상속받는 추상 베이스.
// Player 오브젝트에 컴포넌트로 부착. 기본적으로 비활성화 상태로 시작.
public abstract class CopySkillBase : MonoBehaviour
{
    public CopySkillData data;

    protected PlayerStats stats;
    protected PlayerController controller;

    protected virtual void Awake()
    {
        stats = GetComponent<PlayerStats>();
        controller = GetComponent<PlayerController>();
    }

    /// <summary>에너지 소비 전에 실행 가능 여부 확인. 기본값 true. 서브클래스에서 override.</summary>
    public virtual bool CanExecute() => true;

    /// <summary>CopySkillManager가 호출. 실제 스킬 효과를 구현.</summary>
    public abstract void Execute();
}
