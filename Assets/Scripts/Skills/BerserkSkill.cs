using System.Collections;
using UnityEngine;

// 광폭화 — 일정 시간 이동속도 + 공격속도 증가 (50E)
public class BerserkSkill : CopySkillBase
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float moveSpeedBonus = 3f;
    [SerializeField] private float attackSpeedBonus = 0.5f;

    private bool isActive;
    private Coroutine berserkCoroutine;

    public override void Execute()
    {
        // 재발동 시 타이머 갱신
        if (isActive)
        {
            if (berserkCoroutine != null) StopCoroutine(berserkCoroutine);
            RemoveBuff();
        }
        berserkCoroutine = StartCoroutine(BerserkRoutine());
    }

    IEnumerator BerserkRoutine()
    {
        isActive = true;
        stats.moveSpeed   += moveSpeedBonus;
        stats.attackSpeed += attackSpeedBonus;

        yield return new WaitForSeconds(duration);

        RemoveBuff();
    }

    void RemoveBuff()
    {
        if (!isActive) return;
        stats.moveSpeed   -= moveSpeedBonus;
        stats.attackSpeed -= attackSpeedBonus;
        isActive = false;
        berserkCoroutine = null;
    }
}
