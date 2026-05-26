using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 4스테이지 보스 처치 후 표시되는 엔딩 패널.
/// 같은 씬(Stage4_Ruined)에 비활성 상태로 존재하다 StageManager.LoadEnding이 Show 호출.
/// 표시: "EXPERIMENT COMPLETE" + 클리어 시간 + 런 세포 + 누적 세포 + 메인 메뉴 버튼
/// </summary>
public class VictoryPanel : MonoBehaviour
{
    public static VictoryPanel Instance { get; private set; }

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;     // "EXPERIMENT COMPLETE"
    [SerializeField] private TMP_Text clearTimeText; // "클리어 시간: 12:34"
    [SerializeField] private TMP_Text cellsGainedText; // "획득 세포: +42"
    [SerializeField] private TMP_Text totalCellsText;  // "누적 세포: 87"
    [SerializeField] private TMP_Text statsText;       // 추가 통계 (최종 레벨, 빌드 요약)

    [Header("Buttons")]
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

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    /// <summary>4스 엔딩 진입 시 호출. 시간 정지 + 데이터 표시 + 세포를 메타로 누적.</summary>
    public void Show()
    {
        // 1. 클리어 시간 / 런 세포 미리 캡처 (Accumulate 호출하면 CurrentCells가 0이 됨)
        float clearTime = ComputeClearTime();
        int runCells    = LevelManager.Instance != null ? LevelManager.Instance.CurrentCells : 0;

        // 2. 런 세포를 메타에 누적
        if (GameManager.Instance != null)
            GameManager.Instance.AccumulateRunCellsToMeta();

        // 3. UI 갱신
        if (titleText       != null) titleText.text       = "EXPERIMENT COMPLETE";
        if (clearTimeText   != null) clearTimeText.text   = $"클리어 시간: {FormatTime(clearTime)}";
        if (cellsGainedText != null) cellsGainedText.text = $"획득 세포: +{runCells}";
        if (totalCellsText  != null) totalCellsText.text  = $"누적 세포: {MetaProgressData.TotalCells}";

        // 추가 통계: 최종 레벨 + 빌드 요약 (보유 스킬 수 + 돌연변이 수)
        if (statsText != null)
        {
            int finalLv = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 1;
            int attackCount = 0, passiveCount = 0;
            if (LevelManager.Instance != null)
            {
                foreach (var kv in LevelManager.Instance.GetSkillLevelsCopy())
                {
                    if (kv.Key == null || kv.Value <= 0) continue;
                    if (kv.Key.skillType == SkillType.Attack) attackCount++;
                    else passiveCount++;
                }
            }
            int mutCount = MutationManager.Instance != null ? MutationManager.Instance.GetOwnedIDs().Count : 0;
            int copyCount = 0;
            if (CopySkillManager.Instance != null)
                for (int i = 0; i < 3; i++)
                    if (CopySkillManager.Instance.GetSlot(i) != null) copyCount++;

            statsText.text = $"최종 레벨: <color=#FFD24A>{finalLv}</color>\n" +
                             $"빌드: 공격 {attackCount} / 패시브 {passiveCount} / 카피 {copyCount}\n" +
                             $"돌연변이: {mutCount}개 획득";
        }

        gameObject.SetActive(true);
        Time.timeScale = 0f; // pause
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;

        // 매니저 정리: 메인 메뉴는 클린 상태로 시작해야 함
        CleanupPersistentManagers();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void CleanupPersistentManagers()
    {
        // DontDestroyOnLoad로 살아있는 매니저들을 명시 정리.
        // 메인 메뉴 진입 시 클린 상태 보장.
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

    static float ComputeClearTime()
    {
        // GameManager에 누적 시간이 없으므로 Time.timeSinceLevelLoad로 마지막 스테이지 진행 시간만 사용.
        // 6주차 단순화 — 7주차에 스테이지별 합산 시간 추가 가능.
        return Time.timeSinceLevelLoad;
    }

    static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
