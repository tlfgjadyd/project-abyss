using UnityEngine;

/// <summary>
/// 적 주변 일정 반경 안에 플레이어가 들어오면 일정 간격으로 지속 피해를 입힘.
/// 전기뱀장어(2스테이지), 환경 효과형 적 등에 활용.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
public class EnemyAuraDamage : MonoBehaviour
{
    [Header("범위 지속 피해")]
    [Tooltip("피해를 입히는 반경")]
    [SerializeField] private float radius = 1.5f;
    [Tooltip("초당 입히는 피해량 (tickInterval과 함께 1회 데미지가 결정됨)")]
    [SerializeField] private float damagePerSecond = 3f;
    [Tooltip("피해 적용 주기 (초)")]
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Visuals (선택)")]
    [Tooltip("오라 시각화용 자식 SpriteRenderer가 있으면 자동으로 크기 매칭")]
    [SerializeField] private SpriteRenderer auraVisual;

    private Transform player;
    private EnemyBase enemy;
    private float tickTimer;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();

        if (auraVisual != null)
        {
            float diameter = radius * 2f;
            auraVisual.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        tickTimer = tickInterval;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (player == null || enemy == null) return;
        if (enemy.IsStunned) return;

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;
        tickTimer = tickInterval;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > radius) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.TakeDamage(damagePerSecond * tickInterval);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
