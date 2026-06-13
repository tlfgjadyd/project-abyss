using UnityEngine;

/// <summary>
/// SpriteRenderer 프레임을 fps로 무한 반복 재생. 투사체처럼 살아있는 동안 계속 도는 VFX용.
/// (1회 재생 후 파괴되는 SkillVfxOneShot과 달리 loop) 풀 재사용 시 OnEnable에서 리셋.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimLoop : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 14f;

    private SpriteRenderer sr;
    private int idx;
    private float t;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnEnable()
    {
        idx = 0; t = 0f;
        if (sr != null && frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (sr == null || frames == null || frames.Length < 2) return;
        t += Time.deltaTime;
        float ft = fps > 0f ? 1f / fps : 0.07f;
        while (t >= ft)
        {
            t -= ft;
            idx = (idx + 1) % frames.Length;
            sr.sprite = frames[idx];
        }
    }
}
