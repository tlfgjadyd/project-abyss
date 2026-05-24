using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    // game start
    public enum GameState{Playing, Paused, LevelUp, Mutation, GameOver, StageClear}
    public GameState CurrentState{get;private set;}

    // Stage Timer (Day 45 후속: 카운트다운 → 카운트업으로 변경. 시간 초과 GameOver 폐지)
    [Header("Stage Settings (fallback)")]
    [Tooltip("StageManager.CurrentStage가 있으면 그 값으로 덮어씌워짐. 없을 때만 fallback으로 사용. 현재는 보스 스폰 타이밍에만 의미 있음 (UI는 카운트업).")]
    [SerializeField] private float stageDuration = 540f;

    /// <summary>스테이지 시작 후 경과 시간 (초). 카운트업.</summary>
    public float ElapsedTime { get; private set; }

    /// <summary>호환성: 기존 호출자(VictoryPanel 등)를 위해 유지. 의미는 ElapsedTime과 동일.</summary>
    public float TimeRemaining => ElapsedTime;

    public bool TimerRunning {get; private set;}

    // Event
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<float> OnTimerUpdated;

    void Awake()
    {
        // singleton
        if(Instance != null && Instance !=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {
        StartStage();

        // 스테이지 BGM 자동 재생 (StageManager에서 현재 스테이지 번호 가져옴)
        if (AudioManager.Instance != null && StageManager.Instance != null && StageManager.Instance.CurrentStage != null)
            AudioManager.Instance.PlayStageBGM(StageManager.Instance.CurrentStage.stageNumber);
    }

    void Update()
    {
        if (TimerRunning)
            UpdateTimer();

        // ESC 토글: PausePanel을 직접 켜고 끔. CopySelect/LevelUp 등 다른 패널이 Paused 상태일 때 ESC 무시.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var pp = PausePanel.Instance;
            if (pp != null && pp.IsShowing)
                pp.Hide(); // ResumeGame 내부 호출
            else if (CurrentState == GameState.Playing && pp != null)
                pp.Show(); // PauseGame 내부 호출
        }
    }

    // --- 상태 변환 ----

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        // timeScale/Timer는 이벤트 발행 전에 처리 — 구독자가 또 ChangeState를 호출해도
        // 외부 호출의 switch가 마지막에 덮어쓰지 않도록 atomic하게 유지.
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                TimerRunning = true;
                break;

            case GameState.Paused:
            case GameState.LevelUp:
            case GameState.Mutation:
                Time.timeScale = 0f;
                TimerRunning = false;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                TimerRunning = false;
                AccumulateRunCellsToMeta();
                Debug.Log("Game Over");
                break;

            case GameState.StageClear:
                Time.timeScale = 0f;
                TimerRunning = false;
                // TODO: 다음 스테이지 전환
                Debug.Log("Stage Clear");
                break;
        }

        // 이벤트 발행을 마지막에 — 구독자가 또 ChangeState를 호출하면 그 내부 switch가 최종 적용됨.
        OnGameStateChanged?.Invoke(newState);
    }

    // ── 타이머 ──────────────────────────────────

    void StartStage()
    {
        // stageDuration은 BossSpawner 등에서 보스 스폰 시점 계산에 사용 (StageData.bossSpawnTime).
        // 타이머 자체는 0에서 시작해 계속 카운트업.
        ElapsedTime = 0f;
        ChangeState(GameState.Playing);
    }

    void UpdateTimer()
    {
        ElapsedTime += Time.deltaTime;
        OnTimerUpdated?.Invoke(ElapsedTime);
        // 시간 초과 GameOver 폐지 — 무한 진행. GameOver는 사망 또는 명시 호출로만.
    }

    // ── 외부 호출용 ─────────────────────────────

    public void PauseGame()  => ChangeState(GameState.Paused);
    public void ResumeGame() => ChangeState(GameState.Playing);

    public void TriggerMutation() => ChangeState(GameState.Mutation);

    public void TriggerLevelUp()
    {
        ChangeState(GameState.LevelUp);
        // LevelManager or UI에서 OnGameStateChanged 수신해서 패널 띄움
    }

    public void TriggerGameOver() => ChangeState(GameState.GameOver);
    public void TriggerStageClear() => ChangeState(GameState.StageClear);

    /// <summary>
    /// 현재 런에서 모은 세포를 메타 데이터에 누적하고 인게임 카운터를 비운다.
    /// GameOver / Stage4 엔딩 시점에 한 번씩만 호출.
    /// </summary>
    public void AccumulateRunCellsToMeta()
    {
        var lm = LevelManager.Instance;
        if (lm == null) return;
        int amount = lm.CurrentCells;
        if (amount <= 0) return;

        MetaProgressData.AddCells(amount);
        lm.ConsumeAllCells(); // 인게임 카운터 0으로 (중복 누적 방지)
        Debug.Log($"[GameManager] 메타 세포 +{amount} (누적 {MetaProgressData.TotalCells})");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // StageManager가 있으면 진행 데이터 리셋 + 첫 스테이지로 이동
        if (StageManager.Instance != null)
        {
            StageManager.Instance.StartNewGame();
            return;
        }

        // 폴백: 현재 씬 재로드
        if (PlayerProgressData.Instance != null)
            PlayerProgressData.Instance.ResetAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

