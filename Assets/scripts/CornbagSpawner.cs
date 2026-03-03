using UnityEngine;

/// <summary>
/// Attach this script to any manager GameObject.
/// Assign the cornbag prefab, spawn point, and this will handle
/// spawning a new bag each time the previous one lands.
/// </summary>
public class CornbagSpawner : MonoBehaviour
{
    [Header("Cornbag Settings")]
    [Tooltip("The cornbag prefab to spawn.")]
    [SerializeField] private GameObject cornbagPrefab;

    [Tooltip("The Transform where each new cornbag will be spawned.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Delay (seconds) before spawning the next bag after the previous one lands.")]
    [SerializeField] private float spawnDelay = 0.5f;

    // Tracks how many bags have been spawned (used for naming)
    private int bagCount = 0;

    // Singleton-style static reference so Respawn_Destroy can call back into this
    public static CornbagSpawner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnBag();
    }

    /// <summary>
    /// Spawns a new cornbag at the spawn point with an incremented name.
    /// </summary>
    public void SpawnBag()
    {
        if (cornbagPrefab == null)
        {
            Debug.LogError("CornbagSpawner: No cornbag prefab assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("CornbagSpawner: No spawn point assigned!");
            return;
        }

        bagCount++;
        GameObject newBag = Instantiate(cornbagPrefab, spawnPoint.position, spawnPoint.rotation);
        newBag.name = $"Cornbag_{bagCount}";

        // Give the bag a reference back to this spawner
        Respawn_Destroy respawnScript = newBag.GetComponent<Respawn_Destroy>();
        if (respawnScript == null)
        {
            // Add it automatically if the prefab doesn't already have it
            respawnScript = newBag.AddComponent<Respawn_Destroy>();
        }
        respawnScript.SetSpawner(this);

        Debug.Log($"Spawned: {newBag.name}");
    }

    /// <summary>
    /// Called by Respawn_Destroy after a bag lands. Waits spawnDelay then spawns a new bag.
    /// </summary>
    public void RequestNextBag()
    {
        Invoke(nameof(SpawnBag), spawnDelay);
    }
}
