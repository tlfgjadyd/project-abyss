using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    // game start
    public enum GameState{Playing, Paused, LevelUp, GameOver, StageClear}
    public GameState CurrentState{get;private set;}

    // Stage Timer
    [Header("Stage Settings")]
    [SerializeField] private float stageDuration = 540f; // 9miniute
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

        // 이벤트 던짐
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                TimerRunning = true;
                break;

            case GameState.Paused:
            case GameState.LevelUp:
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
    }

    // ── 타이머 ──────────────────────────────────

    void StartStage()
    {
        TimeRemaining = stageDuration;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

