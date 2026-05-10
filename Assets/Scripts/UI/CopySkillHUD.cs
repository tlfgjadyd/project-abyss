using UnityEngine;

/// <summary>
/// 카피 스킬 슬롯 3개(Q/E/Space)를 묶어서 관리하는 HUD 컴포넌트.
/// CopySkillManager의 슬롯 변경, BioEnergyManager의 에너지 변동을 구독해 갱신하며,
/// CanExecute() 상태 변화는 매 프레임 폴링한다.
/// </summary>
public class CopySkillHUD : MonoBehaviour
{
    [Header("Slot UI (0=Q, 1=E, 2=Space)")]
    [SerializeField] private CopySkillSlotUI[] slotUIs = new CopySkillSlotUI[3];

    [Header("Key Labels")]
    [SerializeField] private string[] keyLabels = { "Q", "E", "Space" };

    private readonly CopySkillData[] currentData = new CopySkillData[3];
    private readonly bool[] lastAvailable = new bool[3];

    void Start()
    {
        // 키 라벨 초기 설정
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null) continue;
            string key = (i < keyLabels.Length) ? keyLabels[i] : "?";
            slotUIs[i].SetKeyLabel(key);
            slotUIs[i].SetEmpty();
            lastAvailable[i] = false;
        }

        // 매니저 이벤트 구독
        if (CopySkillManager.Instance != null)
            CopySkillManager.Instance.OnSlotChanged += OnSlotChanged;

        if (BioEnergyManager.Instance != null)
            BioEnergyManager.Instance.OnEnergyChanged += OnEnergyChanged;

        // 초기 상태 동기화 (테스트 모드 등에서 슬롯이 미리 할당된 경우 대응)
        for (int i = 0; i < slotUIs.Length; i++)
        {
            var skill = CopySkillManager.Instance?.GetSlot(i);
            currentData[i] = skill != null ? skill.data : null;
            if (slotUIs[i] != null)
            {
                if (currentData[i] != null) slotUIs[i].SetSkill(currentData[i]);
                else                        slotUIs[i].SetEmpty();
            }
        }
        RefreshAvailability(force: true);
    }

    void OnDestroy()
    {
        if (CopySkillManager.Instance != null)
            CopySkillManager.Instance.OnSlotChanged -= OnSlotChanged;

        if (BioEnergyManager.Instance != null)
            BioEnergyManager.Instance.OnEnergyChanged -= OnEnergyChanged;
    }

    void Update()
    {
        // CanExecute() 상태 변화 폴링 (Berserk/Dash 활성 변화)
        RefreshAvailability(force: false);
    }

    // ── 이벤트 핸들러 ───────────────────────────

    void OnSlotChanged(int slot, CopySkillData data)
    {
        if (slot < 0 || slot >= slotUIs.Length) return;

        currentData[slot] = data;

        if (slotUIs[slot] == null) return;
        if (data != null) slotUIs[slot].SetSkill(data);
        else              slotUIs[slot].SetEmpty();

        RefreshAvailability(force: true);
    }

    void OnEnergyChanged(float current, float max)
    {
        RefreshAvailability(force: false);
    }

    // ── 갱신 ─────────────────────────────────

    void RefreshAvailability(bool force)
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null) continue;

            bool available = IsSlotAvailable(i);
            if (force || available != lastAvailable[i])
            {
                slotUIs[i].SetAvailable(available);
                lastAvailable[i] = available;
            }
        }
    }

    bool IsSlotAvailable(int slot)
    {
        if (currentData[slot] == null) return false;

        var skill = CopySkillManager.Instance?.GetSlot(slot);
        if (skill == null) return false;

        // CanExecute(): Berserk는 항상 true(재발동 가능), Dash는 isDashing이면 false
        if (!skill.CanExecute()) return false;

        // 의태 기관 돌연변이 시 비용 0 처리
        bool free = CopySkillManager.Instance != null && CopySkillManager.Instance.FreeCopySkills;
        float cost = free ? 0f : currentData[slot].energyCost;

        if (BioEnergyManager.Instance == null) return true;
        return BioEnergyManager.Instance.CanConsume(cost);
    }
}
