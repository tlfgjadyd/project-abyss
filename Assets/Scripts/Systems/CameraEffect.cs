using System.Collections;
using UnityEngine;

/// <summary>
/// 카메라 흔들림 + 줌 효과 매니저.
/// Camera.main을 대상으로 작동. 씬마다 1개 (Camera GameObject에 부착 또는 Auto 자동 부착).
/// 사용: CameraEffect.Instance?.Shake(0.2f, 0.15f); CameraEffect.Instance?.ZoomTo(5.5f, 0.5f);
/// </summary>
public class CameraEffect : MonoBehaviour
{
    public static CameraEffect Instance { get; private set; }

    [Header("Defaults")]
    [Tooltip("Awake에서 저장되는 기본 orthographicSize. ZoomReset 시 복원")]
    [SerializeField] private float defaultOrthoSize = 6.5f;

    private Camera cam;
    private Vector3 basePos;
    private Coroutine shakeCo;
    private Coroutine zoomCo;
    private bool hasBase;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            defaultOrthoSize = cam.orthographicSize;
            basePos = cam.transform.localPosition;
            hasBase = true;
        }
    }

    void OnEnable()
    {
        // 씬 재로드 시 cam 참조 갱신
        if (cam == null) cam = Camera.main;
        if (cam != null && !hasBase)
        {
            basePos = cam.transform.localPosition;
            defaultOrthoSize = cam.orthographicSize;
            hasBase = true;
        }
    }

    // ── Shake ────────────────────────────────────

    /// <summary>amount: 진폭 (world units). duration: 흔들림 시간 (초).</summary>
    public void Shake(float amount, float duration)
    {
        if (cam == null) return;
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(amount, duration));
    }

    IEnumerator ShakeRoutine(float amount, float duration)
    {
        if (!hasBase) { basePos = cam.transform.localPosition; hasBase = true; }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // hitstop 중에도 흔들림 계속
            float decay = Mathf.Lerp(1f, 0f, elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * amount * decay;
            cam.transform.localPosition = basePos + (Vector3)offset;
            yield return null;
        }
        cam.transform.localPosition = basePos;
        shakeCo = null;
    }

    // ── Zoom ─────────────────────────────────────

    public void ZoomTo(float size, float duration)
    {
        if (cam == null) return;
        if (zoomCo != null) StopCoroutine(zoomCo);
        zoomCo = StartCoroutine(ZoomRoutine(size, duration));
    }

    public void ZoomReset(float duration = 0.5f) => ZoomTo(defaultOrthoSize, duration);

    IEnumerator ZoomRoutine(float target, float duration)
    {
        if (cam == null) yield break;
        float start = cam.orthographicSize;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cam.orthographicSize = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        cam.orthographicSize = target;
        zoomCo = null;
    }
}
