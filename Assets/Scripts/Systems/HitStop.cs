using System.Collections;
using UnityEngine;

/// <summary>
/// 짧은 시간 Time.timeScale=0 → 복원. "히트 스톱" 손맛.
/// 싱글톤 — Camera 또는 [Managers] 자식에 부착해도 됨. 자동 생성도 지원.
/// 사용: HitStop.Instance?.Freeze(0.05f);
///
/// 주의: 게임이 이미 Paused/LevelUp 등 timeScale=0 상태면 무시 (중복 방지).
/// </summary>
public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    private Coroutine routine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Freeze(float duration)
    {
        if (duration <= 0f) return;
        // 이미 정지 상태면 무시 (Paused/LevelUp 등)
        if (Time.timeScale <= 0.01f) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        // 외부에서 정지 상태로 바꿨다면 (Paused) 복원하지 않음
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Time.timeScale = 1f;
        }
        routine = null;
    }
}
