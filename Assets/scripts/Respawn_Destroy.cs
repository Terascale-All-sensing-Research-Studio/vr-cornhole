using Oculus.Interaction;
using UnityEngine;
using System.Collections;

/// <summary>
/// Dual-purpose script:
/// 1. Add to the cornbag PREFAB - tracks grab/release state.
/// 2. Add to Floor and Cornhole Board - detects landing and triggers destroy + respawn.
/// </summary>
public class Respawn_Destroy : MonoBehaviour
{
    [Tooltip("Delay (seconds) before the bag is destroyed after landing.")]
    [SerializeField] private float destroyDelay = 0.5f;

    // Set by CornbagSpawner when this bag is instantiated
    private CornbagSpawner spawner;

    // Grabbable reference (only valid when this script is on the bag itself)
    private Grabbable grabbable;

    // Has the bag been released from the player hand?
    public bool hasBeenThrown = false;

    // Guard against double-destroy
    private bool isDestroying = false;

    void Awake()
    {
        HookGrabbable();
    }

    void OnDestroy()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void HookGrabbable()
    {
        grabbable = GetComponent<Grabbable>();
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }
    }

    public void SetSpawner(CornbagSpawner spawnerRef)
    {
        spawner = spawnerRef;
        HookGrabbable();
    }

    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Unselect)
        {
            hasBeenThrown = true;
            Debug.Log(gameObject.name + ": thrown! hasBeenThrown = true");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cornbag"))
        {
            Respawn_Destroy bagScript = collision.gameObject.GetComponent<Respawn_Destroy>();
            Debug.Log("Collision with: " + collision.gameObject.name + " | hasBeenThrown: " + (bagScript != null ? bagScript.hasBeenThrown.ToString() : "no script"));
            if (bagScript != null && bagScript.hasBeenThrown)
                bagScript.TriggerDestroy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cornbag"))
        {
            Respawn_Destroy bagScript = other.GetComponent<Respawn_Destroy>();
            Debug.Log("Trigger with: " + other.gameObject.name + " | hasBeenThrown: " + (bagScript != null ? bagScript.hasBeenThrown.ToString() : "no script"));
            if (bagScript != null && bagScript.hasBeenThrown)
                bagScript.TriggerDestroy();
        }
    }

    public void TriggerDestroy()
    {
        if (isDestroying) return;
        isDestroying = true;

        Debug.Log(gameObject.name + ": TriggerDestroy called.");

        // Use the spawner singleton to run the coroutine since this object is about to be destroyed
        CornbagSpawner spawnerRef = spawner != null ? spawner : CornbagSpawner.Instance;

        if (spawnerRef != null)
            spawnerRef.StartCoroutine(DestroyThenSpawn(spawnerRef));
        else
        {
            Debug.LogWarning("Respawn_Destroy: No CornbagSpawner found -- next bag will not spawn.");
            Destroy(gameObject, destroyDelay);
        }
    }

    private IEnumerator DestroyThenSpawn(CornbagSpawner spawnerRef)
    {
        // Wait for the destroy delay
        yield return new WaitForSeconds(destroyDelay);

        // Destroy the bag
        Destroy(gameObject);

        // Now that the bag is gone, request the next one
        spawnerRef.RequestNextBag();
    }
}