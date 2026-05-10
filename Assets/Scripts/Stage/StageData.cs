using UnityEngine;

/// <summary>
/// 스테이지별 메타데이터.
/// 각 스테이지(1~4)당 1개 에셋으로 생성. 다음 스테이지 참조로 체인 구성.
/// </summary>
[CreateAssetMenu(fileName = "NewStageData", menuName = "Abyss/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("스테이지 번호 (1~4)")]
    public int stageNumber = 1;

    [Tooltip("로드할 씬 이름 (예: 'Stage1_Lab')")]
    public string sceneName;

    [Tooltip("UI에 표시할 스테이지 이름 (예: '해저 연구소')")]
    public string displayName;

    [Header("환경 설정")]
    [Tooltip("이 스테이지에서 압력 시스템 활성화 여부")]
    public bool pressureEnabled = false;

    [Header("압력 파라미터 (pressureEnabled = true일 때만 사용)")]
    [Tooltip("이 Y좌표 아래부터 압력 시작")]
    public float pressureStartY = -5f;
    [Tooltip("이 Y좌표에서 압력 최대")]
    public float pressureMaxY = -30f;

    [Tooltip("최대 압력 시 이동속도 감소 비율 (0~1). 예: 0.20 = -20%")]
    [Range(0f, 1f)] public float pressureMovePenalty = 0.20f;
    [Tooltip("최대 압력 시 공격속도 감소 비율 (0~1). 예: 0.20 = -20%")]
    [Range(0f, 1f)] public float pressureAttackPenalty = 0.20f;
    [Tooltip("최대 압력 시 경험치 획득 보너스 비율 (0~?). 예: 0.30 = +30%")]
    [Min(0f)] public float pressureExpBonus = 0.30f;

    [Header("타이밍")]
    [Tooltip("스테이지 제한 시간 (초)")]
    public float stageDuration = 540f;

    [Tooltip("보스 등장 시간 (초)")]
    public float bossSpawnTime = 480f;

    [Header("스테이지 체인")]
    [Tooltip("다음 스테이지. null이면 마지막 스테이지로 간주.")]
    public StageData nextStage;

    /// <summary>마지막 스테이지인가? (다음 스테이지가 없는 경우)</summary>
    public bool IsFinalStage => nextStage == null;
}
