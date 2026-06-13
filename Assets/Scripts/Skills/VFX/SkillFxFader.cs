using System.Collections;
using UnityEngine;

/// <summary>
/// 짧은 시간 LineRenderer 알파 페이드 후 자동 Destroy. 스킬 시각 helper.
/// </summary>
public class SkillFxFader : MonoBehaviour
{
    LineRenderer lr;
    Color baseColor;
    float duration;

    public void Init(LineRenderer renderer, Color color, float dur)
    {
        lr = renderer;
        baseColor = color;
        duration = dur;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        float t = 0f;
        while (t < duration && lr != null)
        {
            t += Time.deltaTime;
            float a = 1f - (t / duration);
            var c = baseColor; c.a = a;
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
