using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 간 유지되는 플레이어의 영구 데이터. DontDestroyOnLoad로 씬 전환 시에도 유지된다.
/// 첫 씬에 GameObject로 배치되며, 씬 전환 시 자동으로 Restore가 트리거된다.
///
/// 메타 업그레이드용 세이브 파일(세포 영구 소비 등)과는 별개 시스템이다.
/// </summary>
public class PlayerProgressData : MonoBehaviour
{
    public static PlayerProgressData Instance { get; private set; }

    /// <summary>한 번이라도 Capture가 호출됐는지. 첫 씬 진입 시에는 false라 Restore 안 함.</summary>
    public bool HasCaptured { get; private set; }

    // ── 기본 스탯 (메타 결과 + 인게임 상태) ─────────
    public float currentHp;
    public float pressureResistance;

    // ── 진행 상태 ─────────────────────────────────
    public int currentLevel;
    public float currentExp;
    public float expToNextLevel;
    public int currentCells;

    // ── 일반 스킬 레벨 ───────────────────────────
    /// <summary>SkillData 참조 → 현재 레벨. ScriptableObject 참조는 씬 간 유지됨.</summary>
    public Dictionary<SkillData, int> skillLevels = new Dictionary<SkillData, int>();

    // ── 카피 스킬 슬롯 (CopySkillData 자체 참조) ──
    public CopySkillData qSlotData;
    public CopySkillData eSlotData;
    public CopySkillData spaceSlotData;

    // ── 돌연변이 ─────────────────────────────────
    public List<MutationID> ownedMutationIDs = new List<MutationID>();

    // ── 라이프사이클 ─────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // DontDestroyOnLoad는 root GameObject에만 적용 가능 → 자식이면 root로 이동
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 첫 씬에서는 Capture 안 된 상태이므로 Restore 안 함
        if (!HasCaptured) return;

        // 한 프레임 대기 후 Restore — 모든 매니저의 Awake/OnEnable이 끝나 Instance가 등록된 시점 보장.
        StartCoroutine(RestoreNextFrame());
    }

    IEnumerator RestoreNextFrame()
    {
        yield return null; // 매니저들 Awake/OnEnable 완료 대기
        Restore();
    }

    // ── Capture / Restore ────────────────────────

    /// <summary>현재 씬의 매니저들에서 플레이어 상태를 수집. StageManager.TransitionToNext에서 호출.</summary>
    public void Capture()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                currentHp          = stats.currentHp;
                pressureResistance = stats.pressureResistance;
            }
        }

        if (LevelManager.Instance != null)
        {
            currentLevel   = LevelManager.Instance.CurrentLevel;
            currentExp     = LevelManager.Instance.CurrentExp;
            expToNextLevel = LevelManager.Instance.ExpToNextLevel;
            currentCells   = LevelManager.Instance.CurrentCells;
            skillLevels    = LevelManager.Instance.GetSkillLevelsCopy();
        }

        if (CopySkillManager.Instance != null)
        {
            qSlotData     = CopySkillManager.Instance.GetSlot(0)?.data;
            eSlotData     = CopySkillManager.Instance.GetSlot(1)?.data;
            spaceSlotData = CopySkillManager.Instance.GetSlot(2)?.data;
        }

        if (MutationManager.Instance != null)
            ownedMutationIDs = MutationManager.Instance.GetOwnedIDs();

        HasCaptured = true;
        Debug.Log($"[PlayerProgressData] Captured — Lv{currentLevel} / HP {currentHp:F0} / 세포 {currentCells} / 돌연변이 {ownedMutationIDs.Count}");
    }

    /// <summary>새 씬 진입 시 매니저들에 플레이어 상태를 복원. OnSceneLoaded에서 1프레임 대기 후 자동 호출.</summary>
    public void Restore()
    {
        // 1. 스킬 효과 재적용 (LevelManager.SetState 안에서 OnSkillSelected 발행 → SkillEffectApplier가 누적)
        //    이 시점에 maxHp, attackPower 등이 base 위에 += delta로 누적됨.
        if (LevelManager.Instance != null)
            LevelManager.Instance.SetState(currentLevel, currentExp, expToNextLevel, skillLevels, currentCells);

        // 2. 돌연변이 효과 재적용 (스킬으로 강화된 stats 위에 곱셈 변환)
        if (MutationManager.Instance != null && ownedMutationIDs.Count > 0)
            MutationManager.Instance.RestoreOwnedIDs(ownedMutationIDs);

        // 3. PlayerStats currentHp / 압력 저항 복원 (스킬+돌연변이로 maxHp 결정된 후 Clamp)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.pressureResistance = pressureResistance;
                stats.currentHp = Mathf.Clamp(currentHp, 1f, stats.maxHp);
                stats.OnHpChanged?.Invoke(stats.currentHp, stats.maxHp);
            }
        }

        // 4. 카피 스킬 슬롯 복원
        if (CopySkillManager.Instance != null)
        {
            CopySkillManager.Instance.RestoreSlot(0, qSlotData);
            CopySkillManager.Instance.RestoreSlot(1, eSlotData);
            CopySkillManager.Instance.RestoreSlot(2, spaceSlotData);
        }

        Debug.Log($"[PlayerProgressData] Restored — Lv{currentLevel} / HP {currentHp:F0} / 세포 {currentCells}");
    }

    /// <summary>새 게임 시작 시 호출. 모든 진행 데이터 초기화.</summary>
    public void ResetAll()
    {
        HasCaptured = false;
        currentHp = 0f;
        pressureResistance = 0f;
        currentLevel = 0;
        currentExp = 0f;
        expToNextLevel = 0f;
        currentCells = 0;
        skillLevels.Clear();
        qSlotData = null;
        eSlotData = null;
        spaceSlotData = null;
        ownedMutationIDs.Clear();
    }
}
