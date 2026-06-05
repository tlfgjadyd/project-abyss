using UnityEngine;

/// <summary>
/// 산갈치 마디 chain follow — 보스(head) 이동 시 segments가 일렬로 따라옴.
///
/// 핵심: Awake에서 segments를 SetParent(null)로 분리. 자식 관계 유지하면 부모 transform 이동에
/// segment가 자동 평행이동하여 chain follow 결과가 무효화됨 (일직선 유지). 분리 후 OarfishBody가
/// 매 FixedUpdate에 world position 직접 관리.
///
/// 마디 정렬(orientSegments): 각 마디를 체인 진행 방향으로 회전 + sine 유영 웨이브를 더해
/// 위/아래 이동 시에도 수평 에셋이 세로로 쌓이지 않고 휘어지는 뱀처럼 헤엄친다.
/// 좌향(머리가 -X를 봄) sprite 기준이라 진행 방향이 왼쪽이면 flipY로 볏이 아래로 뒤집히지 않게 보정.
///
/// boss 사망 시 OnDisable에서 segments도 함께 비활성화 (cleanup).
/// </summary>
[DefaultExecutionOrder(100)]
public class OarfishBody : MonoBehaviour
{
    [Tooltip("마디 사이 간격 (world units)")]
    [SerializeField] private float spacing = 1.5f;

    [Tooltip("자동 수집되는 자식 segments (Awake 시점). 비어있으면 'Segment'로 시작하는 자식 자동 수집")]
    [SerializeField] private Transform[] segments;

    [Header("유영 모션")]
    [Tooltip("마디를 체인 진행 방향으로 회전시킬지")]
    [SerializeField] private bool orientSegments = true;
    [Tooltip("유영 웨이브 진폭 (진행 방향의 수직으로 출렁임, world units)")]
    [SerializeField] private float waveAmplitude = 0.12f;
    [Tooltip("유영 웨이브 속도")]
    [SerializeField] private float waveFrequency = 3.5f;
    [Tooltip("마디 간 웨이브 위상차 (값이 클수록 S자가 촘촘)")]
    [SerializeField] private float wavePhasePerSegment = 0.55f;
    [Tooltip("꼬리 쪽 축소 비율 (머리=1, 꼬리=이 값)")]
    [SerializeField] private float tailScale = 0.72f;

    private Vector3[] segmentWorldScale; // 분리 후 원래 world scale 보존
    private Vector3[] basePos;           // 웨이브 적용 전 체인 base 위치
    private SpriteRenderer[] segSR;
    private SpriteRenderer headSR;
    private Rigidbody2D headRb;

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

        headSR = GetComponent<SpriteRenderer>();
        headRb = GetComponent<Rigidbody2D>();

        // 2) 부모-자식 분리 — chain follow가 정상 작동하도록.
        //    분리 전 lossyScale 보존 → SetParent(null) 후 localScale로 복원 (부모 scale 영향 사라지므로 보정)
        int n = segments.Length;
        segmentWorldScale = new Vector3[n];
        segSR = new SpriteRenderer[n];
        basePos = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            if (segments[i] == null) continue;
            segmentWorldScale[i] = segments[i].lossyScale;
        }
        for (int i = 0; i < n; i++)
        {
            var seg = segments[i];
            if (seg == null) continue;
            seg.SetParent(null, true); // worldPositionStays=true → 분리 시점 world pos 유지

            // 꼬리로 갈수록 축소(taper) 적용한 world scale 복원
            float t = n > 1 ? (float)i / (n - 1) : 0f;
            seg.localScale = segmentWorldScale[i] * Mathf.Lerp(1f, tailScale, t);

            segSR[i]   = seg.GetComponent<SpriteRenderer>();
            basePos[i] = seg.position;
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
            Vector3 p = transform.position + baseDir * spacing * (i + 1);
            segments[i].position = p;
            if (basePos != null) basePos[i] = p;
            segments[i].gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (segments == null || segments.Length == 0) return;

        // 머리 좌우 flip — sprite가 좌향(-X) 기본이므로 오른쪽 이동 시 flipX
        if (headSR != null && headRb != null)
        {
            float vx = headRb.velocity.x;
            if (Mathf.Abs(vx) > 0.05f) headSR.flipX = vx > 0f;
        }

        // head 위치를 기준 — segments[0]이 head 추적
        Vector3 prev = transform.position;
        int n = segments.Length;
        for (int i = 0; i < n; i++)
        {
            var seg = segments[i];
            if (seg == null) continue;

            Vector3 cur = basePos[i];
            Vector3 diff = prev - cur;
            float dist = diff.magnitude;
            if (dist > spacing)
            {
                // 초과한 만큼만 끌어당김 (간격은 spacing 유지)
                cur = cur + diff.normalized * (dist - spacing);
            }
            basePos[i] = cur;

            Vector3 finalPos = cur;
            Vector3 dir = prev - cur;
            if (orientSegments && dir.sqrMagnitude > 0.0001f)
            {
                Vector3 dn = dir.normalized;
                float angle = Mathf.Atan2(dn.y, dn.x) * Mathf.Rad2Deg;
                seg.rotation = Quaternion.Euler(0f, 0f, angle);
                if (segSR[i] != null) segSR[i].flipY = dn.x < 0f; // 왼쪽 진행 시 볏이 아래로 안 뒤집히게

                // 유영 웨이브 — 진행 방향의 수직으로 출렁 (마디마다 위상차)
                Vector3 perp = new Vector3(-dn.y, dn.x, 0f);
                float wave = Mathf.Sin(Time.time * waveFrequency + i * wavePhasePerSegment) * waveAmplitude;
                finalPos = cur + perp * wave;
            }

            seg.position = finalPos;
            prev = cur; // 체인 연결은 웨이브 적용 전(base) 위치 기준 — 누적 방지
        }
    }
}
