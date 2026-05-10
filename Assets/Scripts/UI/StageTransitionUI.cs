using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 전환 시 페이드 아웃/인 + 스테이지 이름 자막을 처리하는 UI.
///
/// 흐름:
///   PlayFadeOut(displayName, onComplete)
///     → 검은 화면 alpha 0 → 1
///     → 자막 텍스트 표시 (페이드 인 → 유지 → 페이드 아웃)
///     → onComplete 콜백 (주로 SceneManager.LoadScene 호출)
///
///   PlayFadeIn()
///     → 검은 화면 alpha 1 → 0 (새 씬 시작 시)
///
/// [TODO 5주차]
///   - 실제 페이드 코루틴 구현
///   - 화면 풀스크린 검은 Image + 중앙 TMP_Text UI 만들기
///   - StageManager에서 호출하도록 연결
/// </summary>
public class StageTransitionUI : MonoBehaviour
{
    public static StageTransitionUI Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private CanvasGroup fadePanel;       // 풀스크린 검은 패널의 CanvasGroup
    [SerializeField] private TMP_Text    stageNameText;   // 자막용 텍스트

    [Header("타이밍")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float titleHoldDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 1.0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기 상태: 투명 + 자막 비활성
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }
        if (stageNameText != null) stageNameText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 페이드 아웃 + 자막 표시 → onComplete 콜백.
    /// onComplete에서 SceneManager.LoadScene 호출 권장.
    /// </summary>
    public void PlayFadeOut(string stageDisplayName, Action onComplete)
    {
        StartCoroutine(FadeOutRoutine(stageDisplayName, onComplete));
    }

    /// <summary>새 씬 시작 시 페이드 인.</summary>
    public void PlayFadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    // ── 코루틴 (스켈레톤) ─────────────────────────

    IEnumerator FadeOutRoutine(string title, Action onComplete)
    {
        // [TODO 5주차] 실제 alpha 0→1 트윈
        // float t = 0f;
        // while (t < fadeOutDuration) { ... fadePanel.alpha = t/fadeOutDuration; ... }
        // 자막 표시 (title)
        // yield return new WaitForSecondsRealtime(titleHoldDuration);

        Debug.Log($"[StageTransitionUI] FadeOut + '{title}' — TODO: 5주차 구현");
        yield return null;
        onComplete?.Invoke();
    }

    IEnumerator FadeInRoutine()
    {
        // [TODO 5주차] alpha 1→0 트윈
        Debug.Log("[StageTransitionUI] FadeIn — TODO: 5주차 구현");
        yield return null;
    }
}
