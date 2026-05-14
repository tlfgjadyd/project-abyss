using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 진행을 추적하고 다음 스테이지로 전환하는 매니저.
/// DontDestroyOnLoad로 씬 전환 시에도 유지된다.
///
/// 흐름 (목표):
///   보스 처치 → 카피 스킬 선택 → CopySkillSelectCardUI.OnClick()
///       → GameManager.TriggerStageClear()
///       → StageManager.Instance.TransitionToNext() ← 새로 추가될 호출
///       → PlayerProgressData.Capture()
///       → StageTransitionUI.PlayFadeOut(displayName)
///       → SceneManager.LoadScene(nextStage.sceneName)
///       → 새 씬 Start: PlayerProgressData.Restore() + StageTransitionUI.PlayFadeIn()
///
/// [TODO 5주차]
///   - StageTransitionUI 연계
///   - SceneManager.LoadScene 연동
///   - 마지막 스테이지 처리 (엔딩 씬)
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("현재 스테이지")]
    [Tooltip("게임 시작 시 첫 스테이지 데이터 (Stage1_Lab의 StageData 에셋 할당)")]
    [SerializeField] private StageData startingStage;

    public StageData CurrentStage { get; private set; }

    // 스테이지 변경 이벤트 (UI 갱신 등에 사용 가능)
    public System.Action<StageData> OnStageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // DontDestroyOnLoad는 root GameObject에만 적용 가능 → 자식이면 root로 이동
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (CurrentStage == null && startingStage != null)
            CurrentStage = startingStage;
    }

    /// <summary>
    /// 다음 스테이지로 전환. 마지막 스테이지면 엔딩 씬으로.
    /// 페이드 아웃 → 씬 로드 → (새 씬에서 자동 페이드 인).
    /// 씬이 BuildSettings에 없으면 현재 씬 재로드(검증 모드).
    /// </summary>
    public void TransitionToNext()
    {
        // 1. 현재 진행 데이터 저장
        if (PlayerProgressData.Instance != null)
            PlayerProgressData.Instance.Capture();

        // 2. 다음 씬과 자막 결정
        StageData next = CurrentStage != null ? CurrentStage.nextStage : null;
        bool isEnding  = (next == null);

        string sceneName;
        string displayName;

        if (isEnding)
        {
            sceneName   = "Ending";  // TODO 6주차: 엔딩 씬 작성 시 이 이름 유지/변경
            displayName = "엔딩";
            Debug.Log("[StageManager] 마지막 스테이지 클리어 → 엔딩 진입 시도");
        }
        else
        {
            sceneName   = !string.IsNullOrEmpty(next.sceneName) ? next.sceneName : SceneManager.GetActiveScene().name;
            displayName = next.displayName;
            Debug.Log($"[StageManager] '{(CurrentStage != null ? CurrentStage.displayName : "?")}' → '{displayName}' 전환");

            CurrentStage = next;
            OnStageChanged?.Invoke(next);
        }

        // 3. BuildSettings에 없으면 현재 씬으로 fallback (검증 모드)
        int buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + sceneName + ".unity");
        if (buildIndex < 0)
        {
            Debug.LogWarning($"[StageManager] '{sceneName}' 씬이 BuildSettings에 없습니다 → 현재 씬 재로드 (검증 모드).");
            sceneName = SceneManager.GetActiveScene().name;
        }

        // 4. 페이드 아웃 → 씬 로드
        string sceneToLoad = sceneName; // 클로저 캡처용
        if (StageTransitionUI.Instance != null)
        {
            StageTransitionUI.Instance.PlayFadeOut(displayName, () =>
            {
                SceneManager.LoadScene(sceneToLoad);
            });
        }
        else
        {
            // 폴백: 페이드 없이 즉시 로드
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// 엔딩 씬으로 전환. 마지막 보스(연구소장) 처치 후 호출.
    /// [TODO 5주차] 엔딩 씬 이름 결정 + 로드
    /// </summary>
    public void LoadEnding()
    {
        Debug.Log("[StageManager] 엔딩 진입 — TODO: 5주차 구현");
        // SceneManager.LoadScene("Ending");
    }

    /// <summary>새 게임 시작. 진행 데이터 리셋 후 첫 스테이지로.</summary>
    public void StartNewGame()
    {
        if (PlayerProgressData.Instance != null)
            PlayerProgressData.Instance.ResetAll();

        CurrentStage = startingStage;
        OnStageChanged?.Invoke(CurrentStage);

        // 씬 이름 결정: startingStage.sceneName이 BuildSettings에 있으면 사용, 없으면 현재 씬 fallback
        string sceneName = (CurrentStage != null && !string.IsNullOrEmpty(CurrentStage.sceneName))
            ? CurrentStage.sceneName
            : SceneManager.GetActiveScene().name;

        int buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + sceneName + ".unity");
        if (buildIndex < 0)
        {
            Debug.LogWarning($"[StageManager] '{sceneName}' 씬이 BuildSettings에 없어 현재 씬을 재로드합니다.");
            sceneName = SceneManager.GetActiveScene().name;
        }

        SceneManager.LoadScene(sceneName);
    }
}
