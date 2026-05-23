using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 일시정지 패널. 씬에 활성 상태로 저장하고 Start에서 SetActive(false) 호출하여 초기 숨김.
/// (비활성 상태로 저장하면 Awake/Start가 호출 안 돼 Instance 등록 실패.)
///
/// 표시는 GameManager가 ESC 입력 처리할 때 PausePanel.Instance.Show()로 명시 호출.
/// OnGameStateChanged 구독은 사용하지 않음 (CopySkillSelectPanel 등 다른 곳에서 PauseGame 호출 시
/// 일시정지 패널이 같이 뜨는 문제 방지).
/// </summary>
public class PausePanel : MonoBehaviour
{
    public static PausePanel Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsShowing => gameObject.activeSelf;

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
    }

    /// <summary>ESC 입력 시 GameManager가 호출. 패널 켜고 일시정지.</summary>
    public void Show()
    {
        gameObject.SetActive(true);
        GameManager.Instance.PauseGame();
    }

    /// <summary>ESC 입력 또는 "계속하기" 클릭 시 호출. 패널 끄고 재개.</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    void OnResumeClicked()
    {
        Hide();
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
