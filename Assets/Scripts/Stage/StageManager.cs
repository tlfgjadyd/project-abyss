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
        DontDestroyOnLoad(gameObject);

        if (CurrentStage == null && startingStage != null)
            CurrentStage = startingStage;
    }

    /// <summary>
    /// 다음 스테이지로 전환. 마지막 스테이지면 엔딩으로.
    /// [TODO 5주차] 페이드 + 씬 로딩 실제 구현
    /// </summary>
    public void TransitionToNext()
    {
        if (CurrentStage == null)
        {
            Debug.LogError("[StageManager] CurrentStage가 null입니다.");
            return;
        }

        // 1. 현재 진행 데이터 저장
        if (PlayerProgressData.Instance != null)
            PlayerProgressData.Instance.Capture();

        // 2. 마지막 스테이지면 엔딩
        if (CurrentStage.IsFinalStage)
        {
            LoadEnding();
            return;
        }

        // 3. 다음 스테이지로 전환
        StageData next = CurrentStage.nextStage;
        Debug.Log($"[StageManager] '{CurrentStage.displayName}' → '{next.displayName}' 전환");

        CurrentStage = next;
        OnStageChanged?.Invoke(next);

        // [TODO 5주차] 페이드 아웃 → 씬 로드 → 페이드 인
        // StageTransitionUI.Instance.PlayFadeOut(next.displayName, () => {
        //     SceneManager.LoadScene(next.sceneName);
        // });

        // 임시: 직접 씬 로드 (페이드 없이)
        if (!string.IsNullOrEmpty(next.sceneName))
        {
            // SceneManager.LoadScene(next.sceneName);  // 5주차에 활성화
            Debug.Log($"[StageManager] (TODO) SceneManager.LoadScene(\"{next.sceneName}\") 호출 예정");
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

        if (CurrentStage != null && !string.IsNullOrEmpty(CurrentStage.sceneName))
        {
            // SceneManager.LoadScene(CurrentStage.sceneName);  // 5주차에 활성화
            Debug.Log($"[StageManager] (TODO) 새 게임 시작: {CurrentStage.sceneName}");
        }
    }
}
