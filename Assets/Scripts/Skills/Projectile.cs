using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public bool isPiercing;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float hitKnockback = 2f;

    private Rigidbody2D rb;
    private IObjectPool<Projectile> pool;
    private Vector2 direction;
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        timer = lifetime;
    }

    public void SetPool(IObjectPool<Projectile> pool) => this.pool = pool;

    public void Fire(Vector2 dir)
    {
        direction = dir.normalized;
        rb.velocity = direction * speed;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Return();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;

        col.GetComponent<IDamageable>()?.TakeDamage(damage);
        col.GetComponent<Rigidbody2D>()?.AddForce(direction * hitKnockback, ForceMode2D.Impulse);

        if (!isPiercing)
            Return();
    }

    void Return()
    {
        rb.velocity = Vector2.zero;
        pool?.Release(this);
    }
}
