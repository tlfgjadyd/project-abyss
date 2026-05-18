using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 진입점. 메인 패널 / 메타 업그레이드 패널 토글.
/// START → Stage1_Lab 로드 (PlayerProgressData.ResetAll 후).
/// META UPGRADE → MetaUpgradePanel 활성.
/// QUIT → Application.Quit (에디터에서는 EditorApplication.isPlaying = false).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject metaPanel;

    [Header("Main Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button metaButton;
    [SerializeField] private Button quitButton;

    [Header("Meta Panel Buttons")]
    [SerializeField] private Button metaBackButton;

    [Header("Config")]
    [SerializeField] private string firstStageSceneName = "Stage1_Lab";

    void Start()
    {
        ShowMain();

        if (startButton    != null) startButton.onClick.AddListener(OnStart);
        if (metaButton     != null) metaButton.onClick.AddListener(ShowMeta);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);
        if (metaBackButton != null) metaBackButton.onClick.AddListener(ShowMain);
    }

    void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (metaPanel != null) metaPanel.SetActive(false);
    }

    void ShowMeta()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (metaPanel != null) metaPanel.SetActive(true);
    }

    void OnStart()
    {
        // 인게임 진행 데이터는 매번 클린 상태로. (이전 런 잔여 차단)
        if (PlayerProgressData.Instance != null)
            PlayerProgressData.Instance.ResetAll();

        SceneManager.LoadScene(firstStageSceneName);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
