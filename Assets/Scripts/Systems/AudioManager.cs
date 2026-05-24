using UnityEngine;

/// <summary>
/// SFX 식별자. 새 효과음 추가 시 enum 확장 + 인스펙터에서 AudioClip 슬롯 할당.
/// 사운드 자산은 외부 조달 (freesound.org / Unity Asset Store) — 시스템만 우선 구축.
/// </summary>
public enum SfxId
{
    PlayerHit,
    EnemyDeath,
    LevelUp,
    CopySkillCast,
    BossPhase2,
    UIClick,
}

/// <summary>
/// 사운드 매니저 — BGM 1트랙 + SFX 풀.
/// 싱글톤 + DontDestroyOnLoad. [Managers] prefab 자식으로 부착.
/// 자산 미할당 시 무음으로 안전하게 동작 (null 체크).
///
/// 최소 구현 — 외부 자산 조달 후 인스펙터에서 슬롯 채움.
/// 추후 확장 후보: 마스터/BGM/SFX 볼륨 슬라이더, 페이드, 3D 사운드.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume (0~1)")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume    = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume    = 0.9f;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip stage1BGM;
    [SerializeField] private AudioClip stage2BGM;
    [SerializeField] private AudioClip stage3BGM;
    [SerializeField] private AudioClip stage4BGM;
    [SerializeField] private AudioClip bossBGM;

    [System.Serializable]
    public struct SfxEntry { public SfxId id; public AudioClip clip; [Range(0f, 1f)] public float volumeScale; }
    [Header("SFX Clips (id 별 1개)")]
    [SerializeField] private SfxEntry[] sfxEntries;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 자식 GameObject가 아니라 root여야 DontDestroyOnLoad 가능 → 상위 [Managers]가 root라 OK
        // 별도 처리 불필요 (다른 매니저들과 동일 트리)

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    // ── BGM ──────────────────────────────────────

    /// <summary>스테이지 번호(1~4) 또는 메뉴/보스에 따른 BGM 재생. null이면 무음.</summary>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) return;
        if (clip == null) { bgmSource.Stop(); return; }
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();
    }

    public void PlayStageBGM(int stageNumber)
    {
        AudioClip clip = null;
        switch (stageNumber)
        {
            case 1: clip = stage1BGM; break;
            case 2: clip = stage2BGM; break;
            case 3: clip = stage3BGM; break;
            case 4: clip = stage4BGM; break;
        }
        PlayBGM(clip);
    }

    public void PlayMainMenuBGM() => PlayBGM(mainMenuBGM);
    public void PlayBossBGM()     => PlayBGM(bossBGM);
    public void StopBGM()         { if (bgmSource != null) bgmSource.Stop(); }

    // ── SFX ──────────────────────────────────────

    public void PlaySFX(SfxId id)
    {
        if (sfxSource == null || sfxEntries == null) return;
        for (int i = 0; i < sfxEntries.Length; i++)
        {
            if (sfxEntries[i].id != id) continue;
            var entry = sfxEntries[i];
            if (entry.clip == null) return;
            float v = sfxVolume * masterVolume * Mathf.Max(0.0001f, entry.volumeScale);
            sfxSource.PlayOneShot(entry.clip, v);
            return;
        }
    }

    // ── Volume ───────────────────────────────────

    public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume; }
    public void SetBgmVolume(float v)    { bgmVolume    = Mathf.Clamp01(v); if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume; }
    public void SetSfxVolume(float v)    { sfxVolume    = Mathf.Clamp01(v); }
}
