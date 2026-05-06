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
