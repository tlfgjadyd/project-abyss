using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 일시정지 패널. 씬에 비활성 상태로 존재하다 GameManager가 Paused로 진입하면 표시.
/// GameManager.OnGameStateChanged 구독 → Paused 진입 시 표시, 탈출 시 숨김.
/// "계속하기" 버튼은 GameManager.ResumeGame 호출. "메인 메뉴"는 매니저 정리 후 MainMenu 로드.
/// </summary>
public class PausePanel : MonoBehaviour
{
    public static PausePanel Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        gameObject.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    void OnGameStateChanged(GameManager.GameState state)
    {
        gameObject.SetActive(GameManager.Instance.CurrentState == GameManager.GameState.Paused);
    }

    void OnResumeClicked()
    {
        GameManager.Instance.ResumeGame();
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        CleanupPersistentManagers();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void CleanupPersistentManagers()
    {
        // VictoryPanel과 동일 패턴 — DontDestroyOnLoad 매니저 명시 정리
        if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);
        if (LevelManager.Instance != null) Destroy(LevelManager.Instance.gameObject);
        if (StageManager.Instance != null) Destroy(StageManager.Instance.gameObject);
        if (BioEnergyManager.Instance != null) Destroy(BioEnergyManager.Instance.gameObject);
        if (CopySkillManager.Instance != null) Destroy(CopySkillManager.Instance.gameObject);
        if (PlayerProgressData.Instance != null)
        {
            PlayerProgressData.Instance.ResetAll();
            Destroy(PlayerProgressData.Instance.gameObject);
        }
    }
}
