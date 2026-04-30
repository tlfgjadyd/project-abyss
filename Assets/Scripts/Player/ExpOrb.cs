using UnityEngine;
using UnityEngine.Pool;

public class ExpOrb : MonoBehaviour
{
    public float expAmount;

    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private float moveSpeed = 6f;

    private Transform player;
    private IObjectPool<ExpOrb> pool;

    public void SetPool(IObjectPool<ExpOrb> pool) => this.pool = pool;

    void OnEnable()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) < attractRadius)
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        LevelManager.Instance.AddExp(expAmount);
        pool.Release(this);
    }
}
