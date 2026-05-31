using System.Collections;
using UnityEngine;

/// <summary>
/// 짧은 원형 폭발 + 페이드. 적 사망 시 자체 생성 후 destroy.
/// LineRenderer 기반 — sprite/particle system 의존성 없음.
/// 사용: DeathExplosion.Spawn(pos, color, radius=0.6f);
/// </summary>
public class DeathExplosion : MonoBehaviour
{
    public static void Spawn(Vector3 position, Color color, float maxRadius = 0.6f, float duration = 0.25f)
    {
        var go = new GameObject("DeathExplosion");
        go.transform.position = position;
        var de = go.AddComponent<DeathExplosion>();
        de.color = color;
        de.maxRadius = maxRadius;
        de.duration = duration;
    }

    Color color;
    float maxRadius;
    float duration;

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        var lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        int seg = 20;
        lr.positionCount = seg + 1;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float r = Mathf.Lerp(0.1f, maxRadius, p);
            float a = 1f - p;
            Color c = color; c.a = a;
            lr.startColor = c; lr.endColor = c;
            for (int i = 0; i <= seg; i++)
            {
                float ang = i / (float)seg * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f));
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
