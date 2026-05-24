using System.Collections;
using UnityEngine;

/// <summary>
/// 연구소장 보호막 — HP 25% 도달 시 일정 시간 무적 + 시각 표시.
/// 보호막 해제 후 BossDirectorAttack에 신호 (현재는 단순 무적만, 행동 변화는 BossDirectorAttack 자체의 IsPhase2 사용).
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossDirectorShield : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private float shieldDuration = 5f;

    [Header("Visual")]
    [SerializeField] private Color shieldColor = new Color(0.4f, 0.9f, 1f, 0.9f);
    [SerializeField] private float ringRadius = 1.2f;
    [SerializeField] private float ringWidth = 0.15f;

    private BossBase boss;
    private GameObject fxObj;
    private bool triggered;

    void Awake() { boss = GetComponent<BossBase>(); }

    void OnEnable()
    {
        triggered = false;
        if (boss != null) boss.OnHpQuarterReached += Activate;
    }

    void OnDisable()
    {
        if (boss != null) boss.OnHpQuarterReached -= Activate;
        if (fxObj != null) { Destroy(fxObj); fxObj = null; }
        if (boss != null) boss.IsInvincible = false;
    }

    void Activate()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(ShieldRoutine());
    }

    IEnumerator ShieldRoutine()
    {
        boss.IsInvincible = true;
        SpawnFx();
        Debug.Log("[BossDirectorShield] 보호막 활성");

        yield return new WaitForSeconds(shieldDuration);

        boss.IsInvincible = false;
        if (fxObj != null) { Destroy(fxObj); fxObj = null; }
        Debug.Log("[BossDirectorShield] 보호막 해제");
    }

    void SpawnFx()
    {
        fxObj = new GameObject("DirectorShieldFx");
        fxObj.transform.SetParent(transform, false);
        fxObj.transform.localPosition = Vector3.zero;
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.startColor = shieldColor;
        lr.endColor = shieldColor;
        int seg = 36;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * ringRadius, Mathf.Sin(a) * ringRadius, 0f));
        }
    }
}
