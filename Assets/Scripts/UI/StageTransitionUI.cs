using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 검은 화면 상태로 새 씬에 진입했다면 자동 페이드 인
        if (fadePanel != null && fadePanel.alpha > 0.5f)
            PlayFadeIn();
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

    // ── 코루틴 ─────────────────────────────────

    IEnumerator FadeOutRoutine(string title, Action onComplete)
    {
        if (fadePanel == null)
        {
            Debug.LogError("[StageTransitionUI] fadePanel이 null입니다.");
            onComplete?.Invoke();
            yield break;
        }

        // 입력 차단 + 자막 초기 비활성
        fadePanel.blocksRaycasts = true;
        if (stageNameText != null)
        {
            stageNameText.text = title;
            stageNameText.gameObject.SetActive(false);
            var c = stageNameText.color;
            c.a = 0f;
            stageNameText.color = c;
        }

        // 1) 검은 화면 alpha 0 → 1 (unscaledDeltaTime — timeScale=0에서도 동작)
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Clamp01(t / fadeOutDuration);
            yield return null;
        }
        fadePanel.alpha = 1f;

        // 2) 자막 페이드 인 (검은 화면 위에서, 짧게)
        if (stageNameText != null)
        {
            stageNameText.gameObject.SetActive(true);
            float titleFadeIn = 0.3f;
            float tt = 0f;
            while (tt < titleFadeIn)
            {
                tt += Time.unscaledDeltaTime;
                var c = stageNameText.color;
                c.a = Mathf.Clamp01(tt / titleFadeIn);
                stageNameText.color = c;
                yield return null;
            }
            var fc = stageNameText.color;
            fc.a = 1f;
            stageNameText.color = fc;
        }

        // 3) 자막 hold
        yield return new WaitForSecondsRealtime(titleHoldDuration);

        // 4) onComplete (보통 SceneManager.LoadScene). 페이드 인은 새 씬에서 호출됨.
        onComplete?.Invoke();
    }

    IEnumerator FadeInRoutine()
    {
        if (fadePanel == null)
        {
            Debug.LogError("[StageTransitionUI] fadePanel이 null입니다.");
            yield break;
        }

        // 진입 직후 한 프레임 대기 (새 씬 매니저 Start 완료 보장)
        yield return null;

        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;

        // alpha 1 → 0
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = 1f - Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;

        if (stageNameText != null) stageNameText.gameObject.SetActive(false);
    }
}
