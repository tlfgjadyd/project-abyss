using UnityEngine;
using UnityEngine.Pool;

public class ExpOrb : MonoBehaviour
{
    public float expAmount;

    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private float moveSpeed = 6f;

    private Transform player;
    private PlayerStats playerStats;
    private IObjectPool<ExpOrb> pool;

    public void SetPool(IObjectPool<ExpOrb> pool) => this.pool = pool;

    void OnEnable()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (player == null) return;

        // 자기 유도 패시브로 흡인 범위 배율 적용
        float effRadius = attractRadius * (playerStats != null ? playerStats.magneticRangeMultiplier : 1f);
        if (Vector2.Distance(transform.position, player.position) < effRadius)
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        LevelManager.Instance.AddExp(expAmount);
        pool.Release(this);
    }
}
