using UnityEngine;

/// <summary>보스 처치 후 카피 스킬을 선택하는 패널.</summary>
public class CopySkillSelectPanel : MonoBehaviour
{
    public static CopySkillSelectPanel Instance { get; private set; }

    [SerializeField] private CopySkillSelectCardUI[] cards; // 슬롯 최대 3개

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => gameObject.SetActive(false);

    public void Show(BossData bossData)
    {
        if (bossData == null)
        {
            Debug.LogError("[CopySkillSelectPanel] bossData가 null입니다.");
            return;
        }
        if (bossData.copySkillOptions == null || bossData.copySkillOptions.Length == 0)
        {
            Debug.LogError($"[CopySkillSelectPanel] '{bossData.bossName}'의 copySkillOptions가 비어있습니다.");
            return;
        }
        if (cards == null || cards.Length == 0)
        {
            Debug.LogError("[CopySkillSelectPanel] cards 배열이 비어있습니다. 인스펙터에서 카드 슬롯을 할당하세요.");
            return;
        }

        gameObject.SetActive(true);
        GameManager.Instance.PauseGame();

        var options = bossData.copySkillOptions;
        for (int i = 0; i < cards.Length; i++)
        {
            bool hasCard = i < options.Length;
            cards[i].gameObject.SetActive(hasCard);
            if (hasCard)
                cards[i].Setup(options[i], bossData.copySkillSlot);
        }
    }

    public void Hide() => gameObject.SetActive(false);
}
