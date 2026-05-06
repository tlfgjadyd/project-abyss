using UnityEngine;

/// <summary>
/// 경과 시간이 bossSpawnTime에 도달하면 보스를 스폰.
/// 보스 등장 시 일반 적 스폰 중단, 보스 사망 시 카피 스킬 선택 UI 표시.
/// </summary>
public class BossSpawner : MonoBehaviour
{
    [Header("보스 설정")]
    [SerializeField] private GameObject   bossPrefab;
    [Tooltip("스테이지 시작 후 보스가 등장하는 시간 (초)")]
    [SerializeField] private float        bossSpawnTime = 480f;
    [Tooltip("플레이어로부터 보스가 스폰될 거리")]
    [SerializeField] private float        spawnOffset   = 10f;

    [Header("연결")]
    [SerializeField] private EnemySpawner enemySpawner;

    private float   elapsed;
    private bool    bossSpawned;
    private BossBase currentBoss;

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (bossSpawned) return;

        elapsed += Time.deltaTime;
        if (elapsed >= bossSpawnTime)
            SpawnBoss();
    }

    void SpawnBoss()
    {
        bossSpawned = true;

        // 일반 적 스폰 중단
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        // 스폰 위치: 플레이어 오른쪽 offset
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2 pos = playerObj != null
            ? (Vector2)playerObj.transform.position + Vector2.right * spawnOffset
            : Vector2.zero;

        var bossObj = Instantiate(bossPrefab, pos, Quaternion.identity);
        currentBoss = bossObj.GetComponent<BossBase>();

        if (currentBoss != null)
        {
            currentBoss.OnBossDeath += OnBossDied;
            BossHPBar.Instance?.Show(currentBoss);
            Debug.Log($"[BossSpawner] {currentBoss.Data.bossName} 등장!");
        }
    }

    void OnBossDied(BossBase boss)
    {
        BossHPBar.Instance?.Hide();
        CopySkillSelectPanel.Instance?.Show(boss.Data);
    }
}
