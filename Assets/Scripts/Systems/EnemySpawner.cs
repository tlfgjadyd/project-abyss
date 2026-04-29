using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public float interval = 2f;
        public int poolSize = 20;
    }

    [SerializeField] private Wave[] waves;
    [SerializeField] private float spawnRadius = 12f;

    private Transform player;
    private float[] timers;

    // 간단한 오브젝트 풀 (wave별)
    private GameObject[][] pools;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        timers = new float[waves.Length];
        pools = new GameObject[waves.Length][];

        for (int i = 0; i < waves.Length; i++)
        {
            pools[i] = new GameObject[waves[i].poolSize];
            for (int j = 0; j < waves[i].poolSize; j++)
            {
                pools[i][j] = Instantiate(waves[i].enemyPrefab);
                pools[i][j].SetActive(false);

                var eb = pools[i][j].GetComponent<EnemyBase>();
                if (eb != null)
                    eb.OnDeath += OnEnemyDeath;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        for (int i = 0; i < waves.Length; i++)
        {
            timers[i] -= Time.deltaTime;
            if (timers[i] <= 0f)
            {
                SpawnEnemy(i);
                timers[i] = waves[i].interval;
            }
        }
    }

    void SpawnEnemy(int waveIndex)
    {
        if (player == null) return;

        GameObject obj = GetPooled(waveIndex);
        if (obj == null) return;

        obj.transform.position = GetSpawnPosition();
        obj.SetActive(true);
    }

    Vector2 GetSpawnPosition()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        return (Vector2)player.position + dir * spawnRadius;
    }

    GameObject GetPooled(int waveIndex)
    {
        foreach (var obj in pools[waveIndex])
            if (!obj.activeInHierarchy) return obj;
        return null;
    }

    void OnEnemyDeath(EnemyBase enemy)
    {
        // 경험치 드롭은 Day 5~6 LevelManager 연결 시 추가
    }
}
