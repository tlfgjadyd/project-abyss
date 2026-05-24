using UnityEngine;

/// <summary>
/// 산갈치 마디 chain follow — 보스(head) 이동 시 segments가 일렬로 따라옴.
///
/// 핵심: Awake에서 segments를 SetParent(null)로 분리. 자식 관계 유지하면 부모 transform 이동에
/// segment가 자동 평행이동하여 chain follow 결과가 무효화됨 (일직선 유지). 분리 후 OarfishBody가
/// 매 FixedUpdate에 world position 직접 관리.
///
/// boss 사망 시 OnDisable에서 segments도 함께 비활성화 (cleanup).
///
/// 일반적인 procedural snake 패턴 — 뱀서 류·인디에서 가장 흔한 구현.
/// </summary>
[DefaultExecutionOrder(100)]
public class OarfishBody : MonoBehaviour
{
    [Tooltip("마디 사이 간격 (world units)")]
    [SerializeField] private float spacing = 1.1f;

    [Tooltip("자동 수집되는 자식 segments (Awake 시점). 비어있으면 'Segment'로 시작하는 자식 자동 수집")]
    [SerializeField] private Transform[] segments;

    private Vector3[] segmentWorldScale; // 분리 후 원래 world scale 보존

    void Awake()
    {
        // 1) 자동 수집
        if (segments == null || segments.Length == 0)
        {
            var list = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                if (c.name.StartsWith("Segment")) list.Add(c);
            }
            segments = list.ToArray();
        }

        // 2) 부모-자식 분리 — chain follow가 정상 작동하도록.
        //    분리 전 lossyScale 보존 → SetParent(null) 후 localScale로 복원 (부모 scale 영향 사라지므로 보정)
        segmentWorldScale = new Vector3[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            segmentWorldScale[i] = segments[i].lossyScale;
        }
        for (int i = 0; i < segments.Length; i++)
        {
            var seg = segments[i];
            if (seg == null) continue;
            seg.SetParent(null, true); // worldPositionStays=true → 분리 시점 world pos 유지
            seg.localScale = segmentWorldScale[i]; // world scale 복원
        }
    }

    void OnDisable()
    {
        // 보스 사망/풀 반환 시 segments도 비활성화 (씬에 잔존 방지)
        if (segments == null) return;
        foreach (var seg in segments)
        {
            if (seg != null) seg.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        // 부활 (현재 시스템엔 없지만 안전상): segments 위치를 head 뒤로 재정렬 + 활성화
        if (segments == null) return;
        Vector3 baseDir = -transform.up; // head 아래 방향 기본
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            segments[i].position = transform.position + baseDir * spacing * (i + 1);
            segments[i].gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (segments == null || segments.Length == 0) return;

        // head 위치를 기준 — segments[0]이 head 추적
        Vector3 prev = transform.position;
        for (int i = 0; i < segments.Length; i++)
        {
            var seg = segments[i];
            if (seg == null) continue;
            Vector3 cur = seg.position;
            Vector3 diff = prev - cur;
            float dist = diff.magnitude;
            if (dist > spacing)
            {
                // 초과한 만큼만 끌어당김 (간격은 spacing 유지)
                seg.position = cur + diff.normalized * (dist - spacing);
            }
            prev = seg.position;
        }
    }
}
