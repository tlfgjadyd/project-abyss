using System.Collections;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;

    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    void OnEnable()
    {
        // 풀에서 재사용 시 색상 초기화
        if (sr != null)
            sr.color = originalColor;
    }

    public void PlayFlash()
    {
        if (sr == null) return;

        // 이미 플래시 중이면 색상 복원 후 새로 시작 (이전 플래시의 flashColor가 캡처되지 않도록)
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            sr.color = baseColorBeforeFlash;
        }

        // 플래시 직전 현재 색상을 캡처 → 종료 후 이 색상으로 복귀
        // (페이즈2 톤 등 외부에서 적용된 색상을 보존하기 위함)
        baseColorBeforeFlash = sr.color;

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private Color baseColorBeforeFlash;

    IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = baseColorBeforeFlash;
        flashCoroutine = null;
    }
}
