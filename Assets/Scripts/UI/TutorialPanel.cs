using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 첫 플레이 시 1회만 표시되는 조작 안내 패널.
/// Stage1_Lab에 부착. PlayerPrefs.Abyss_TutorialSeen=1이면 자동 비활성.
/// 표시 중에는 GameManager를 Paused로 두고, 확인 버튼 클릭 시 ResumeGame + 플래그 저장.
/// </summary>
public class TutorialPanel : MonoBehaviour
{
    const string PrefKey = "Abyss_TutorialSeen";

    [SerializeField] private Button confirmButton;

    void Start()
    {
        // 이미 본 적 있으면 즉시 비활성화
        if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        // 첫 플레이 — 게임 일시정지 후 패널 표시
        gameObject.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
    }

    void OnConfirm()
    {
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    /// <summary>외부에서 튜토리얼 플래그 리셋 (개발/테스트용).</summary>
    public static void ResetSeen()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
    }
}
