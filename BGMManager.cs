using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Threading; // Used for modern C# / Unity 6 Cancellation

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Audio Setup")]
    [SerializeField, Tooltip("Primary AudioSource. Auto-created if empty.")]
    private AudioSource primarySource;
    [SerializeField, Tooltip("Secondary AudioSource. Auto-created if empty.")]
    private AudioSource secondarySource;
    private bool isPrimaryActive = true;

    [Header("Playlist & Sequence")]
    [SerializeField, Tooltip("Songs to play in order if not overridden by triggers.")]
    private List<SongProfile> playlist = new List<SongProfile>();
    [SerializeField] private bool loopPlaylist = true;
    private int currentSongIndex = -1;

    [Header("Transition Customization")]
    [SerializeField, Min(0.1f)] 
    private float crossfadeDuration = 3f;
    [SerializeField, Tooltip("Custom curve for the crossfade volume. Highly customizable for romantic ease-in/ease-out.")]
    private AnimationCurve crossfadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lyrics Customization")]
    [SerializeField] private TextMeshProUGUI lyricsText;
    [SerializeField, Min(0.1f)] private float lyricFadeDuration = 0.8f;
    [SerializeField, Tooltip("Custom curve for text fade in/out.")]
    private AnimationCurve lyricFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Color lyricBaseColor = Color.white;
    
    [SerializeField, Tooltip("How much to scale the text during the transition for that romantic push effect (1.0 = no scale)")]
    private float lyricScaleTarget = 1.05f;

    // Modern C# token for cancelling tasks safely when switching songs rapidly
    private CancellationTokenSource playbackCts;
    private SongProfile currentSongProfile;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSources()
    {
        if (primarySource == null) primarySource = gameObject.AddComponent<AudioSource>();
        if (secondarySource == null) secondarySource = gameObject.AddComponent<AudioSource>();
        
        primarySource.loop = false;
        secondarySource.loop = false;
        primarySource.playOnAwake = false;
        secondarySource.playOnAwake = false;
        primarySource.volume = 0f;
        secondarySource.volume = 0f;

        if (lyricsText != null) SetLyricsAlpha(0f);
    }

    private void Start()
    {
        if (playlist.Count > 0)
        {
            PlayNextInPlaylist();
        }
    }

    private void Update()
    {
        AudioSource activeSource = isPrimaryActive ? primarySource : secondarySource;
        if (activeSource.isPlaying && activeSource.clip != null)
        {
            if (activeSource.time >= activeSource.clip.length - crossfadeDuration && playlist.Count > 0)
            {
                PlayNextInPlaylist();
            }
        }
    }

    public void PlayNextInPlaylist()
    {
        if (playlist.Count == 0) return;

        currentSongIndex++;
        if (currentSongIndex >= playlist.Count)
        {
            if (loopPlaylist) currentSongIndex = 0;
            else return;
        }

        PlaySong(playlist[currentSongIndex]);
    }

    public void PlaySong(SongProfile songToPlay)
    {
        if (songToPlay == null || songToPlay.audioClip == null) return;
        if (currentSongProfile == songToPlay) return;

        currentSongProfile = songToPlay;

        AudioSource activeSource = isPrimaryActive ? primarySource : secondarySource;
        AudioSource nextSource = isPrimaryActive ? secondarySource : primarySource;

        isPrimaryActive = !isPrimaryActive;

        // Safely cancel any currently running transitions before starting new ones
        playbackCts?.Cancel();
        playbackCts?.Dispose();
        playbackCts = new CancellationTokenSource();

        // Unity 6: Using 'Awaitable' instead of Coroutines for highly optimized performance 
        // and modern standard C# await mechanics.
        _ = CrossfadeAsync(activeSource, nextSource, songToPlay, playbackCts.Token);
        _ = HandleLyricsAsync(songToPlay, playbackCts.Token);
    }

    // Leveraging Unity 6 Awaitables
    private async Awaitable CrossfadeAsync(AudioSource fadingOutSource, AudioSource fadingInSource, SongProfile newSong, CancellationToken token)
    {
        float timer = 0f;
        float startVolumeOut = fadingOutSource.volume;
        float targetVolumeIn = newSong.volume;

        fadingInSource.clip = newSong.audioClip;
        fadingInSource.Play();
        fadingInSource.volume = 0f;

        try
        {
            while (timer < crossfadeDuration)
            {
                token.ThrowIfCancellationRequested(); // Native cancellation check

                timer += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(timer / crossfadeDuration);
                
                // Evaluated on the custom AnimationCurve from Inspector!
                float curveValue = crossfadeCurve.Evaluate(normalizedTime);

                fadingOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, curveValue);
                fadingInSource.volume = Mathf.Lerp(0f, targetVolumeIn, curveValue);

                await Awaitable.NextFrameAsync(token);
            }

            fadingOutSource.volume = 0f;
            fadingOutSource.Stop();
            fadingInSource.volume = targetVolumeIn;
        }
        catch (System.OperationCanceledException)
        {
            // Transition was gracefully aborted because a new song was triggered
        }
    }

    private async Awaitable HandleLyricsAsync(SongProfile song, CancellationToken token)
    {
        if (lyricsText == null || song.lyrics == null || song.lyrics.Count == 0) return;

        CancellationTokenSource hideCts = null;
        try
        {
            await FadeLyricAlphaAsync(0f, token);
            await Awaitable.WaitForSecondsAsync(song.lyricsCooldown, token);

            AudioSource activeSource = isPrimaryActive ? primarySource : secondarySource;
            int currentLyricIndex = 0;

            while (activeSource.isPlaying)
            {
                token.ThrowIfCancellationRequested();

                if (currentLyricIndex < song.lyrics.Count)
                {
                    LyricLine nextLyric = song.lyrics[currentLyricIndex];

                    if (activeSource.time >= nextLyric.timestamp)
                    {
                        hideCts?.Cancel();
                        hideCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        
                        _ = ShowLyricAsync(nextLyric.text, hideCts.Token);
                        
                        if (nextLyric.duration > 0f)
                        {
                            _ = HideLyricAfterDurationAsync(nextLyric.duration, hideCts.Token);
                        }
                        
                        currentLyricIndex++;
                    }
                }
                await Awaitable.NextFrameAsync(token);
            }

            hideCts?.Cancel();
            await FadeLyricAlphaAsync(0f, token);
        }
        catch (System.OperationCanceledException)
        {
            // Handled
        }
        finally
        {
            hideCts?.Dispose();
        }
    }

    private async Awaitable ShowLyricAsync(string text, CancellationToken token)
    {
        try
        {
            await FadeLyricAlphaAsync(0f, token);
            
            lyricsText.text = text;
            // Push-in slightly for romantic typography effect
            lyricsText.transform.localScale = Vector3.one * (2f - lyricScaleTarget); 
            
            float timer = 0;
            while(timer < lyricFadeDuration)
            {
                token.ThrowIfCancellationRequested();

                timer += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(timer / lyricFadeDuration);
                float curveValue = lyricFadeCurve.Evaluate(normalizedTime);
                
                SetLyricsAlpha(curveValue);
                lyricsText.transform.localScale = Vector3.Lerp(Vector3.one * (2f - lyricScaleTarget), Vector3.one * lyricScaleTarget, curveValue);
                
                await Awaitable.NextFrameAsync(token);
            }
            
            SetLyricsAlpha(1f);
            lyricsText.transform.localScale = Vector3.one * lyricScaleTarget;
        }
        catch (System.OperationCanceledException) { }
    }

    private async Awaitable HideLyricAfterDurationAsync(float duration, CancellationToken token)
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(duration, token);
            await FadeLyricAlphaAsync(0f, token);
        }
        catch (System.OperationCanceledException) { }
    }

    private async Awaitable FadeLyricAlphaAsync(float targetAlpha, CancellationToken token)
    {
        if (lyricsText == null) return;

        float startAlpha = lyricsText.color.a;
        float timer = 0f;

        try
        {
            while (timer < lyricFadeDuration)
            {
                token.ThrowIfCancellationRequested();

                timer += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(timer / lyricFadeDuration);
                float curveValue = lyricFadeCurve.Evaluate(normalizedTime);

                SetLyricsAlpha(Mathf.Lerp(startAlpha, targetAlpha, curveValue));
                await Awaitable.NextFrameAsync(token);
            }

            SetLyricsAlpha(targetAlpha);
        }
        catch (System.OperationCanceledException) { }
    }

    private void SetLyricsAlpha(float alpha)
    {
        if (lyricsText == null) return;
        Color c = lyricBaseColor;
        c.a = alpha;
        lyricsText.color = c;
    }

    private void OnDestroy()
    {
        // Absolute clean up for memory safety
        playbackCts?.Cancel();
        playbackCts?.Dispose();
    }
}
