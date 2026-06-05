using UnityEngine;

/// <summary>
/// 산갈치(단일 긴 sprite) 방향 정렬 — 보스 전체를 진행 방향으로 회전시켜
/// 긴 리본 몸이 헤엄치는 쪽을 향하게 한다. 좌향(머리=-X)·등지느러미 위(+Y) sprite 기준이라
/// 오른쪽으로 갈 때는 flipY로 배가 위로 뒤집히지 않게 보정.
/// rb.velocity(선속도)는 회전과 무관하므로 BossAI 추적 로직에 영향 없음.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class OarfishOrient : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [Tooltip("회전 보간 속도 (도/초)")]
    [SerializeField] private float turnSpeed = 540f;
    [Tooltip("이 속도 이상 움직일 때만 방향 갱신")]
    [SerializeField] private float moveThreshold = 0.05f;
    [Tooltip("sprite가 왼쪽(머리=-X)을 향하면 true")]
    [SerializeField] private bool spriteFacesLeft = true;

    private Rigidbody2D rb;
    private float targetZ;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        targetZ = transform.eulerAngles.z;
    }

    void LateUpdate()
    {
        Vector2 v = rb.velocity;
        if (v.magnitude > moveThreshold)
        {
            float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            targetZ = spriteFacesLeft ? ang - 180f : ang;
            if (sr != null)
                sr.flipY = spriteFacesLeft ? (v.x > 0f) : (v.x < 0f);
        }
        float z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetZ, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }
}
