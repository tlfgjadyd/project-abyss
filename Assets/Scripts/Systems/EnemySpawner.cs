using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        [Tooltip("스폰 간격 (초)")]
        public float interval = 2f;
        public int poolSize = 20;
        [Tooltip("스테이지 시작 후 이 시간(초)이 지나야 스폰 시작")]
        public float startTime = 0f;
    }

    [SerializeField] private Wave[] waves;
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private int maxActiveEnemies = 50;

    private Transform player;
    private float[] timers;
    private float elapsedTime;
    private GameObject[][] pools;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        timers = new float[waves.Length];
        pools  = new GameObject[waves.Length][];

        for (int i = 0; i < waves.Length; i++)
        {
            pools[i] = new GameObject[waves[i].poolSize];
            for (int j = 0; j < waves[i].poolSize; j++)
            {
                pools[i][j] = Instantiate(waves[i].enemyPrefab);
                pools[i][j].SetActive(false);
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        elapsedTime += Time.deltaTime;

        // 시간 경과에 따른 max active / interval 스케일
        float maxActiveScale = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentMaxActiveScale : 1f;
        float intervalScale  = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentIntervalScale  : 1f;

        int currentMaxActive = Mathf.RoundToInt(maxActiveEnemies * maxActiveScale);
        if (GetActiveEnemyCount() >= currentMaxActive)
            return;

        for (int i = 0; i < waves.Length; i++)
        {
            if (elapsedTime < waves[i].startTime) continue;

            timers[i] -= Time.deltaTime;
            if (timers[i] <= 0f)
            {
                SpawnEnemy(i);
                timers[i] = waves[i].interval * intervalScale;
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

    int GetActiveEnemyCount()
    {
        int count = 0;
        foreach (var pool in pools)
            foreach (var obj in pool)
                if (obj != null && obj.activeInHierarchy) count++;
        return count;
    }

    /// <summary>
    /// 풀의 모든 활성 적을 비활성화한다.
    /// 보스 클리어 등 화면 정리가 필요한 시점에 호출.
    /// </summary>
    public void DespawnAll()
    {
        if (pools == null) return;

        foreach (var pool in pools)
        {
            if (pool == null) continue;
            foreach (var obj in pool)
            {
                if (obj != null && obj.activeInHierarchy)
                    obj.SetActive(false);
            }
        }
    }
}
