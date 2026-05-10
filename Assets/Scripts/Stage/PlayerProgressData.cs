using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 간 유지되는 플레이어의 영구 데이터.
/// DontDestroyOnLoad로 씬 전환 시에도 유지된다.
///
/// [TODO 5주차] Capture()/Restore() 실제 구현:
///   - 매니저들에서 현재 상태를 수집/복원하는 로직 작성
///   - 스킬 레벨 직렬화 방식 결정 (string ID 기반 권장)
/// </summary>
public class PlayerProgressData : MonoBehaviour
{
    public static PlayerProgressData Instance { get; private set; }

    // ── 기본 스탯 ─────────────────────────────────
    [Header("기본 스탯 (영구 보존)")]
    public float maxHp;
    public float currentHp;
    public float pressureResistance;

    // ── 진행 상태 ─────────────────────────────────
    [Header("진행 상태")]
    public int currentLevel;
    public float currentExp;

    // ── 일반 스킬 레벨 ───────────────────────────
    /// <summary>일반 스킬 ID(string) → 현재 레벨</summary>
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    // ── 카피 스킬 슬롯 ───────────────────────────
    [Header("카피 스킬 슬롯")]
    public CopySkillID qSlotID     = CopySkillID.None;
    public CopySkillID eSlotID     = CopySkillID.None;
    public CopySkillID spaceSlotID = CopySkillID.None;

    // ── 돌연변이 ─────────────────────────────────
    /// <summary>플레이어가 선택한 돌연변이 ID 목록</summary>
    public List<string> ownedMutationIDs = new List<string>();

    // ── 라이프사이클 ─────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 현재 씬의 매니저들에서 플레이어 상태를 수집.
    /// StageClear 직전 호출.
    /// [TODO 5주차] 매니저별 Getter 호출하여 실제 데이터 수집
    /// </summary>
    public void Capture()
    {
        // 예시 (실제 구현은 5주차):
        // var stats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        // if (stats != null) { maxHp = stats.maxHp; currentHp = stats.currentHp; pressureResistance = stats.pressureResistance; }
        // currentLevel = LevelManager.Instance.CurrentLevel;
        // currentExp   = LevelManager.Instance.CurrentExp;
        // skillLevels  = LevelManager.Instance.GetSkillLevelsCopy();
        // qSlotID     = CopySkillManager.Instance.GetSlot(0)?.data?.copySkillID ?? CopySkillID.None;
        // eSlotID     = CopySkillManager.Instance.GetSlot(1)?.data?.copySkillID ?? CopySkillID.None;
        // spaceSlotID = CopySkillManager.Instance.GetSlot(2)?.data?.copySkillID ?? CopySkillID.None;
        // ownedMutationIDs = MutationManager.Instance.GetOwnedIDs();

        Debug.Log("[PlayerProgressData] Capture() — TODO: 5주차 구현");
    }

    /// <summary>
    /// 새 씬 진입 시 매니저들에 플레이어 상태를 복원.
    /// 새 스테이지 Start에서 호출.
    /// [TODO 5주차] 매니저별 Setter 호출하여 실제 데이터 적용
    /// </summary>
    public void Restore()
    {
        // 예시 (실제 구현은 5주차):
        // var stats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        // if (stats != null) { stats.maxHp = maxHp; stats.currentHp = currentHp; stats.pressureResistance = pressureResistance; }
        // LevelManager.Instance.SetState(currentLevel, currentExp, skillLevels);
        // CopySkillManager.Instance.AssignFromIDs(qSlotID, eSlotID, spaceSlotID);
        // MutationManager.Instance.RestoreOwnedIDs(ownedMutationIDs);

        Debug.Log("[PlayerProgressData] Restore() — TODO: 5주차 구현");
    }

    /// <summary>새 게임 시작 시 호출. 모든 진행 데이터 초기화.</summary>
    public void ResetAll()
    {
        maxHp = 0f;
        currentHp = 0f;
        pressureResistance = 0f;
        currentLevel = 0;
        currentExp = 0f;
        skillLevels.Clear();
        qSlotID = CopySkillID.None;
        eSlotID = CopySkillID.None;
        spaceSlotID = CopySkillID.None;
        ownedMutationIDs.Clear();
    }
}
