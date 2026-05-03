using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;       // "85 / 100" (선택)

    [Header("Exp")]
    [SerializeField] private Slider expSlider;

    [Header("Bio Energy")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;   // "120 / 200" (선택)

    [Header("Info")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;

    void Start()
    {
        // PlayerStats 이벤트 구독
        if (playerStats != null)
        {
            playerStats.OnHpChanged += UpdateHp;
            UpdateHp(playerStats.currentHp, playerStats.maxHp);
        }

        // LevelManager 이벤트 구독
        LevelManager.Instance.OnExpChanged    += UpdateExp;
        LevelManager.Instance.OnLevelChanged  += UpdateLevel;
        GameManager.Instance.OnTimerUpdated   += UpdateTimer;
        BioEnergyManager.Instance.OnEnergyChanged += UpdateEnergy;

        // 초기값 적용
        UpdateLevel(LevelManager.Instance.CurrentLevel);
        UpdateExp(LevelManager.Instance.CurrentExp, LevelManager.Instance.ExpToNextLevel);
        UpdateTimer(GameManager.Instance.TimeRemaining);
        UpdateEnergy(BioEnergyManager.Instance.CurrentEnergy, BioEnergyManager.Instance.MaxEnergy);
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHpChanged -= UpdateHp;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnExpChanged   -= UpdateExp;
            LevelManager.Instance.OnLevelChanged -= UpdateLevel;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimerUpdated -= UpdateTimer;
        if (BioEnergyManager.Instance != null)
            BioEnergyManager.Instance.OnEnergyChanged -= UpdateEnergy;
    }

    // ── 업데이트 ────────────────────────────────

    void UpdateHp(float current, float max)
    {
        if (hpSlider != null) hpSlider.value = max > 0f ? current / max : 0f;
        if (hpText != null)   hpText.text    = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    void UpdateExp(float current, float max)
    {
        if (expSlider != null) expSlider.value = max > 0f ? current / max : 0f;
    }

    void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"Lv. {level}";
    }

    void UpdateTimer(float remaining)
    {
        if (timerText == null) return;
        int min = Mathf.FloorToInt(remaining / 60f);
        int sec = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    void UpdateEnergy(float current, float max)
    {
        if (energySlider != null) energySlider.value = max > 0f ? current / max : 0f;
        if (energyText != null)   energyText.text    = $"{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(max)}";
    }
}
