using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LyricLine
{
    [Tooltip("The time in seconds when this lyric should appear during the song.")]
    [Min(0f)] // Unity Inspector customization: prevents negative time
    public float timestamp;
    
    [Tooltip("The text to display for the romantic lyric.")]
    [TextArea(2, 4)]
    public string text;
    
    [Tooltip("How long this lyric should stay on screen (0 means it stays until the next lyric replaces it).")]
    [Min(0f)]
    public float duration = 0f;
}

[CreateAssetMenu(fileName = "NewSongProfile", menuName = "BGM System/Song Profile", order = 1)]
public class SongProfile : ScriptableObject
{
    [Header("Track Core Setup")]
    public string songName;
    public AudioClip audioClip;
    
    [Tooltip("Volume multiplier to balance different tracks so they sound consistent.")]
    [Range(0f, 1f)] // Slider in inspector for ease of use
    public float volume = 1f;

    [Space(10)]
    [Header("Lyrics Configuration")]
    [Tooltip("Initial delay before the lyric system starts reading the timestamps.")]
    [Min(0f)]
    public float lyricsCooldown = 0f;

    [Tooltip("List of lyrics synchronized with their start times.")]
    public List<LyricLine> lyrics = new List<LyricLine>();
}
