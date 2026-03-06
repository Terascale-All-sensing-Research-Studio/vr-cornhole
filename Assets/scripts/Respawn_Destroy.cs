using Oculus.Interaction;
using UnityEngine;
using System.Collections;

public class Respawn_Destroy : MonoBehaviour
{
    [Tooltip("Delay (seconds) before the bag is destroyed after landing.")]
    [SerializeField] private float destroyDelay = 0.5f;

    private CornbagSpawner spawner;
    private Grabbable grabbable;
    public bool hasBeenThrown = false;
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
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);

        // Only request the next bag if the limit hasn't been reached
        if (!spawnerRef.SpawnLimitReached)
        {
            spawnerRef.RequestNextBag();
        }
        else
        {
            Debug.Log("Spawn limit reached. No more bags will spawn.");
        }
    }
}