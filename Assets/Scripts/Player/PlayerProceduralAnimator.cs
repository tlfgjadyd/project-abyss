using UnityEngine;

/// <summary>
/// 정적 sprite 한 장에 절차적 생명감을 부여한다.
/// 콜라이더/리지드바디가 붙은 루트 transform은 절대 건드리지 않고,
/// 시각 전용 자식(visual)만 움직인다.
///   - idle 호흡: 위아래 둥실거림 + 가로/세로 스쿼시(젤리·문어 느낌)
///   - 이동 시: 위상 가속(둥실 빨라짐) + 진행 방향으로 살짝 기울임
///   - 진행 방향에 따라 flipX
/// 새 sprite/프레임시트로 교체해도 이 컴포넌트는 그대로 동작한다.
/// </summary>
[DisallowMultipleComponent]
public class PlayerProceduralAnimator : MonoBehaviour
{
    [Header("참조 (비우면 자동 탐색)")]
    [Tooltip("시각 전용 자식 Transform. 비우면 첫 SpriteRenderer 자식을 사용")]
    public Transform visual;
    public Rigidbody2D body;

    [Header("호흡 / 둥실거림")]
    [Tooltip("위아래 둥실 진폭 (월드 단위)")]
    public float bobAmplitude = 0.06f;
    [Tooltip("기본 둥실 속도 (정지 시)")]
    public float bobSpeed = 2.2f;
    [Tooltip("이동 속도에 비례한 둥실 속도 가산")]
    public float moveBobBoost = 0.35f;
    [Tooltip("호흡 스쿼시 강도 (0~0.15 권장)")]
    public float breatheAmount = 0.05f;

    [Header("이동 기울임")]
    [Tooltip("최대 기울임 각도(도)")]
    public float maxTilt = 8f;
    [Tooltip("이 수평 속도에서 최대 기울임에 도달")]
    public float tiltRefSpeed = 6f;
    [Tooltip("기울임 보간 속도")]
    public float tiltSmooth = 8f;

    [Header("방향 뒤집기")]
    public bool flipByVelocity = true;
    [Tooltip("원본 sprite가 오른쪽을 향하면 true")]
    public bool spriteFacesRight = true;
    [Tooltip("이 수평 속도 이상에서만 flip (떨림 방지)")]
    public float flipDeadzone = 0.05f;

    SpriteRenderer sr;
    PlayerController controller;
    Vector3 baseLocalPos;
    Vector3 baseLocalScale;
    float phase;
    float curTilt;

    void Awake()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();

        if (visual == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) visual = sr.transform;
        }
        else
        {
            sr = visual.GetComponent<SpriteRenderer>();
        }

        if (visual != null)
        {
            baseLocalPos = visual.localPosition;
            baseLocalScale = visual.localScale;
        }
    }

    void LateUpdate()
    {
        if (visual == null) return;

        Vector2 vel = body != null ? body.velocity : Vector2.zero;
        float speed = vel.magnitude;

        // 위상 진행 — 이동할수록 빨라짐
        phase += (bobSpeed + speed * moveBobBoost) * Time.deltaTime;
        float s = Mathf.Sin(phase);

        // 둥실거림 (위아래)
        Vector3 pos = baseLocalPos;
        pos.y += s * bobAmplitude;
        visual.localPosition = pos;

        // 호흡 스쿼시 — 세로가 늘면 가로가 줆 (부피 보존 느낌)
        float sx = baseLocalScale.x * (1f - s * breatheAmount * 0.5f);
        float sy = baseLocalScale.y * (1f + s * breatheAmount);
        visual.localScale = new Vector3(sx, sy, baseLocalScale.z);

        // 진행 방향으로 기울임 (루트가 freezeRotation이라 안전)
        float targetTilt = -Mathf.Clamp(vel.x / tiltRefSpeed, -1f, 1f) * maxTilt;
        curTilt = Mathf.Lerp(curTilt, targetTilt, Time.deltaTime * tiltSmooth);
        visual.localRotation = Quaternion.Euler(0f, 0f, curTilt);

        // 바라보는 방향에 따라 flip — 입력 기반 facing 사용(넉백/밀림에 영향 안 받음)
        float facingX = (controller != null) ? controller.FaceDirection.x : vel.x;
        if (flipByVelocity && sr != null && Mathf.Abs(facingX) > flipDeadzone)
        {
            sr.flipX = spriteFacesRight ? facingX < 0f : facingX > 0f;
        }
    }
}
