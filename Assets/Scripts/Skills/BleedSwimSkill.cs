using System.Collections;
using UnityEngine;

/// <summary>
/// 절단 유영 — 일정 시간 고속 이동 + 궤적에 출혈 트레일 생성 (130E).
/// 트레일 안의 적은 지속 피해. 10초간 1회 발동.
/// 3스테이지 산갈치 보스 카피 스킬.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BleedSwimSkill : CopySkillBase
{
    [Header("Swim")]
    [SerializeField] private float speedMultiplier = 2.5f;
    [SerializeField] private float duration = 4f;

    [Header("Trail")]
    [Tooltip("트레일 노드 생성 간격 (초)")]
    [SerializeField] private float trailInterval = 0.1f;
    [SerializeField] private float trailRadius = 0.7f;
    [SerializeField] private float trailLifetime = 3f;
    [Tooltip("틱당 데미지 = attackPower * damageMultiplier")]
    [SerializeField] private float damageMultiplier = 0.5f;
    [Tooltip("트레일 데미지 틱 간격")]
    [SerializeField] private float damageTickInterval = 0.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color trailColor = new Color(0.9f, 0.1f, 0.3f, 0.6f);

    private Rigidbody2D rb;
    private bool isSwimming;
    private float baseMoveSpeed;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override bool CanExecute() => !isSwimming;

    public override void Execute()
    {
        StartCoroutine(SwimRoutine());
    }

    IEnumerator SwimRoutine()
    {
        isSwimming = true;
        baseMoveSpeed = stats.moveSpeed;
        stats.moveSpeed = baseMoveSpeed * speedMultiplier;

        float elapsed = 0f;
        float trailTimer = 0f;

        while (elapsed < duration)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f)
            {
                SpawnTrail(transform.position);
                trailTimer = trailInterval;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        stats.moveSpeed = baseMoveSpeed;
        isSwimming = false;
    }

    void SpawnTrail(Vector2 pos)
    {
        var node = new GameObject("BleedTrail");
        node.transform.position = pos;
        var trail = node.AddComponent<BleedTrailNode>();
        float dmg = stats.attackPower * damageMultiplier;
        trail.Initialize(dmg, trailRadius, trailLifetime, damageTickInterval, enemyLayer, trailColor);
    }
}
