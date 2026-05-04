using UnityEngine;

public class CopySkillManager : MonoBehaviour
{
    public static CopySkillManager Instance { get; private set; }

    // 슬롯: Q(0) = 1스테이지, E(1) = 2스테이지, Space(2) = 3스테이지
    private CopySkillBase qSlot;
    private CopySkillBase eSlot;
    private CopySkillBase spaceSlot;

    [Header("Debug")]
    [Tooltip("체크 시 Player의 BerserkSkill→Q, DashSkill→E, HealingFactorSkill→Space 자동 할당")]
    [SerializeField] private bool testMode = false;

    // 슬롯 변경 시 UI 갱신용 이벤트 (slot index, data)
    public System.Action<int, CopySkillData> OnSlotChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (testMode)
            AssignTestSlots();
    }

    void AssignTestSlots()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        qSlot     = player.GetComponent<BerserkSkill>();
        eSlot     = player.GetComponent<DashSkill>();
        spaceSlot = player.GetComponent<HealingFactorSkill>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (Input.GetKeyDown(KeyCode.Q))     TryActivate(qSlot);
        if (Input.GetKeyDown(KeyCode.E))     TryActivate(eSlot);
        if (Input.GetKeyDown(KeyCode.Space)) TryActivate(spaceSlot);
    }

    // ── 슬롯 할당 (보스 처치 후 호출) ───────────────

    /// <summary>slot: 0=Q, 1=E, 2=Space</summary>
    public void AssignSkill(int slot, CopySkillBase skill)
    {
        switch (slot)
        {
            case 0: qSlot     = skill; break;
            case 1: eSlot     = skill; break;
            case 2: spaceSlot = skill; break;
            default: return;
        }
        OnSlotChanged?.Invoke(slot, skill?.data);
    }

    public CopySkillBase GetSlot(int slot) => slot switch
    {
        0 => qSlot,
        1 => eSlot,
        2 => spaceSlot,
        _ => null
    };

    // ── 발동 ─────────────────────────────────────

    void TryActivate(CopySkillBase skill)
    {
        if (skill == null) return;

        if (!skill.CanExecute()) return;

        float cost = skill.data != null ? skill.data.energyCost : 0f;

        if (!BioEnergyManager.Instance.CanConsume(cost))
        {
            Debug.Log($"[CopySkill] 에너지 부족 — {cost}E 필요 / 현재 {BioEnergyManager.Instance.CurrentEnergy}E");
            // TODO: UI 부족 피드백 (게이지 흔들림 등)
            return;
        }

        BioEnergyManager.Instance.ConsumeEnergy(cost);
        skill.Execute();
    }
}
