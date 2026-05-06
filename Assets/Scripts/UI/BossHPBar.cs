using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>보스 전투 중에만 표시되는 보스 전용 HP바.</summary>
public class BossHPBar : MonoBehaviour
{
    public static BossHPBar Instance { get; private set; }

    [SerializeField] private Slider   hpSlider;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text hpText;        // "450 / 600" (선택)

    private BossBase currentBoss;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => gameObject.SetActive(false);

    public void Show(BossBase boss)
    {
        currentBoss = boss;
        gameObject.SetActive(true);

        if (bossNameText != null) bossNameText.text = boss.Data.bossName;

        boss.OnHpChanged += UpdateHP;
        UpdateHP(boss.CurrentHp, boss.MaxHp);
    }

    public void Hide()
    {
        if (currentBoss != null)
        {
            currentBoss.OnHpChanged -= UpdateHP;
            currentBoss = null;
        }
        gameObject.SetActive(false);
    }

    void UpdateHP(float current, float max)
    {
        if (hpSlider    != null) hpSlider.value = max > 0f ? current / max : 0f;
        if (hpText      != null) hpText.text    = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}
