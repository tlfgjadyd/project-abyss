using System.Collections;
using UnityEngine;

/// <summary>
/// 1회성 스킬 VFX. 프레임 배열을 fps로 한 번 재생하고 (옵션) 페이드아웃 후 자기 파괴.
/// 프레임이 1장이든 여러 장이든 동일하게 처리 → 모든 일반 공격 스킬 VFX에 재사용.
///
/// 사용: Resources/VFX/*.prefab (SpriteRenderer + 이 컴포넌트, frames 할당)를
/// 스킬이 Instantiate → 위치/회전/스케일만 지정. Animator/Controller 불필요.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SkillVfxOneShot : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 16f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private float fadeDuration = 0.08f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        if (sr == null || frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Color baseColor = sr.color;
        float frameTime = fps > 0f ? 1f / fps : 0.05f;

        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        if (fadeOut && fadeDuration > 0f)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(baseColor.a, 0f, t / fadeDuration);
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
