using UnityEngine;
using UnityEngine.Pool;

public class ExpOrbPool : MonoBehaviour
{
    public static ExpOrbPool Instance { get; private set; }

    [SerializeField] private ExpOrb expOrbPrefab;
    [SerializeField] private int defaultCapacity = 30;
    [SerializeField] private int maxSize = 100;

    private IObjectPool<ExpOrb> pool;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        pool = new ObjectPool<ExpOrb>(
            createFunc:      () => { var orb = Instantiate(expOrbPrefab); orb.SetPool(pool); return orb; },
            actionOnGet:     orb => orb.gameObject.SetActive(true),
            actionOnRelease: orb => orb.gameObject.SetActive(false),
            actionOnDestroy: orb => Destroy(orb.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize:         maxSize
        );
    }

    public void Spawn(Vector2 position, float expAmount)
    {
        var orb = pool.Get();
        orb.transform.position = position;
        orb.expAmount = expAmount;
    }
}
