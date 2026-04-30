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
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
        flashCoroutine = null;
    }
}
