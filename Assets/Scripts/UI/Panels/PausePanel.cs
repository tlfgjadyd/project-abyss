using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 일시정지 패널. 씬에 활성 상태로 저장하고 Start에서 SetActive(false) 호출하여 초기 숨김.
/// (비활성 상태로 저장하면 Awake/Start가 호출 안 돼 Instance 등록 실패.)
///
/// 표시는 GameManager가 ESC 입력 처리할 때 PausePanel.Instance.Show()로 명시 호출.
/// Show() 호출 시 보유 스킬/카피/돌연변이 리스트 갱신. 호버 시 우측 툴팁 영역에 description 표시.
/// </summary>
public class PausePanel : MonoBehaviour
{
    public static PausePanel Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Skills Display")]
    [Tooltip("보유 항목 리스트가 추가될 부모. 비어있으면 표시 안 됨.")]
    [SerializeField] private RectTransform skillsList;
    [Tooltip("호버 시 표시될 툴팁 텍스트 (제목)")]
    [SerializeField] private TMP_Text tooltipTitle;
    [Tooltip("호버 시 표시될 툴팁 텍스트 (설명)")]
    [SerializeField] private TMP_Text tooltipDesc;
    [Tooltip("동적 생성되는 항목 라벨에 적용할 폰트")]
    [SerializeField] private TMP_FontAsset font;

    public bool IsShowing => gameObject.activeSelf;

    private readonly List<GameObject> entryGOs = new List<GameObject>();

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

    /// <summary>ESC 입력 시 GameManager가 호출. 패널 켜고 일시정지 + 보유 스킬 리스트 갱신.</summary>
    public void Show()
    {
        gameObject.SetActive(true);
        GameManager.Instance.PauseGame();
        RefreshSkillList();
        ClearTooltip();
    }

    /// <summary>ESC 입력 또는 "계속하기" 클릭 시 호출. 패널 끄고 재개.</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    void OnResumeClicked() => Hide();

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        CleanupPersistentManagers();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void CleanupPersistentManagers()
    {
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

    // ── 보유 스킬 리스트 ─────────────────────────

    void RefreshSkillList()
    {
        if (skillsList == null) return;

        // 기존 항목 정리
        foreach (var go in entryGOs) if (go != null) Destroy(go);
        entryGOs.Clear();

        // 1) 일반 스킬 (Lv1+)
        var lvls = LevelManager.Instance != null ? LevelManager.Instance.GetSkillLevelsCopy() : null;
        if (lvls != null)
        {
            foreach (var kv in lvls)
            {
                if (kv.Key == null || kv.Value <= 0) continue;
                AddEntry(
                    $"{kv.Key.skillName} Lv.{kv.Value}",
                    $"{kv.Key.skillName} (Lv.{kv.Value}/{kv.Key.maxLevel}) — {kv.Key.skillType}",
                    kv.Key.description
                );
            }
        }

        // 2) 카피 스킬
        if (CopySkillManager.Instance != null)
        {
            string[] slotKeys = { "Q", "E", "Space" };
            for (int i = 0; i < 3; i++)
            {
                var sk = CopySkillManager.Instance.GetSlot(i);
                if (sk == null || sk.data == null) continue;
                AddEntry(
                    $"[{slotKeys[i]}] {sk.data.skillName}",
                    $"[{slotKeys[i]}] {sk.data.skillName} — 에너지 {sk.data.energyCost:0}",
                    sk.data.description
                );
            }
        }

        // 3) 돌연변이
        if (MutationManager.Instance != null)
        {
            var ids = MutationManager.Instance.GetOwnedIDs();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var data = FindMutationData(id);
                    if (data == null) continue;
                    AddEntry(
                        $"★ {data.mutationName}",
                        $"★ 돌연변이 — {data.mutationName}",
                        data.description + (string.IsNullOrEmpty(data.penaltyDescription) ? "" : "\n패널티: " + data.penaltyDescription)
                    );
                }
            }
        }
    }

    MutationData FindMutationData(MutationID id)
    {
        var pool = Resources.FindObjectsOfTypeAll<MutationData>();
        foreach (var m in pool) if (m != null && m.mutationID == id) return m;
        return null;
    }

    void AddEntry(string label, string tooltipTitleStr, string tooltipDescStr)
    {
        var entryGo = new GameObject("Entry_" + label, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(LayoutElement));
        entryGo.transform.SetParent(skillsList, false);
        var img = entryGo.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        var le = entryGo.GetComponent<LayoutElement>();
        le.minHeight = 32f;
        le.preferredHeight = 32f;

        // 라벨 텍스트
        var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        lblGo.transform.SetParent(entryGo.transform, false);
        var lrt = lblGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(8, 0); lrt.offsetMax = new Vector2(-8, 0);
        var lt = lblGo.GetComponent<TextMeshProUGUI>();
        lt.text = label;
        if (font != null) lt.font = font;
        lt.fontSize = 18;
        lt.alignment = TextAlignmentOptions.Left;
        lt.color = Color.white;

        // 호버 이벤트
        var trigger = entryGo.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) => SetTooltip(tooltipTitleStr, tooltipDescStr));
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) => ClearTooltip());
        trigger.triggers.Add(exit);

        entryGOs.Add(entryGo);
    }

    void SetTooltip(string title, string desc)
    {
        if (tooltipTitle != null) tooltipTitle.text = title;
        if (tooltipDesc  != null) tooltipDesc.text  = desc;
    }

    void ClearTooltip()
    {
        if (tooltipTitle != null) tooltipTitle.text = "스킬에 마우스를 올려보세요";
        if (tooltipDesc  != null) tooltipDesc.text  = "";
    }
}
