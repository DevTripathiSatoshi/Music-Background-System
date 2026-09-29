using UnityEngine;
using UnityEngine.Events; // Useful for triggering visual events perfectly synced with song start

[RequireComponent(typeof(Collider))]
public class BGMTrigger : MonoBehaviour
{
    [Header("Trigger Setup")]
    [Tooltip("The song profile to smoothly transition to when triggered.")]
    public SongProfile songToTrigger;

    [Tooltip("Should this only ever fire once per gameplay session?")]
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    [Header("Trigger Conditions")]
    [Tooltip("Tag of the object allowed to trigger this (e.g. 'Player').")]
    public string triggerTag = "Player";

    [Header("Integration / Workflow (Optional)")]
    [Tooltip("Fires precisely when the song is successfully triggered. Great for linking up particle effects, dialogue, or cutscenes!")]
    public UnityEvent onSongTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            TriggerSong();
        }
    }

    private void TriggerSong()
    {
        if (hasTriggered && triggerOnce) return;

        if (songToTrigger != null && BGMManager.Instance != null)
        {
            BGMManager.Instance.PlaySong(songToTrigger);
            hasTriggered = true;
            
            // Invoke the custom Unity Event
            onSongTriggered?.Invoke();
        }
        else
        {
            if (BGMManager.Instance == null) Debug.LogWarning("BGMTrigger couldn't find BGMManager.Instance in the scene!");
            if (songToTrigger == null) Debug.LogWarning($"BGMTrigger on {gameObject.name} has no SongProfile assigned.");
        }
    }

    // Optional: Make it easier to see in the editor scene view
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.8f, 0.3f); // Romantic pink transparency
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box) Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider sphere) Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
    }
}
