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

    // Stage Timer
    [Header("Stage Settings (fallback)")]
    [Tooltip("StageManager.CurrentStage가 있으면 그 값으로 덮어씌워짐. 없을 때만 fallback으로 사용.")]
    [SerializeField] private float stageDuration = 540f;
    public float TimeRemaining {get; private set;}
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
    }

    void Update()
    {
        if (TimerRunning)
            UpdateTimer();

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
                // TODO: GameOver UI 호출
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
        // StageManager가 있고 현재 스테이지 데이터가 있으면 그 값을 우선 사용
        float duration = stageDuration;
        if (StageManager.Instance != null && StageManager.Instance.CurrentStage != null)
            duration = StageManager.Instance.CurrentStage.stageDuration;

        TimeRemaining = duration;
        ChangeState(GameState.Playing);
    }

    void UpdateTimer()
    {
        TimeRemaining -= Time.deltaTime;
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            // 시간 초과 → 일단 GameOver (보스 미처치 시)
            ChangeState(GameState.GameOver);
        }
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

