using UnityEngine;

public class CornbagSpawner : MonoBehaviour
{
    [Header("Cornbag Settings")]
    [Tooltip("The cornbag prefab to spawn.")]
    [SerializeField] private GameObject cornbagPrefab;
    [Tooltip("The Transform where each new cornbag will be spawned.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Delay (seconds) before spawning the next bag after the previous one lands.")]
    [SerializeField] private float spawnDelay = 0.5f;
    [Tooltip("Maximum number of bags to spawn before stopping.")]
    [SerializeField] private int maxBags = 20;

    private int bagCount = 0;
    public static CornbagSpawner Instance { get; private set; }

    public bool SpawnLimitReached => bagCount >= maxBags;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnBag();
    }

    public void SpawnBag()
    {
        if (SpawnLimitReached)
        {
            Debug.Log($"CornbagSpawner: Spawn limit of {maxBags} reached. No more bags will spawn.");
            return;
        }

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

        Respawn_Destroy respawnScript = newBag.GetComponent<Respawn_Destroy>();
        if (respawnScript == null)
            respawnScript = newBag.AddComponent<Respawn_Destroy>();

        respawnScript.SetSpawner(this);
        Debug.Log($"Spawned: {newBag.name} ({bagCount}/{maxBags})");
    }

    public void RequestNextBag()
    {
        if (SpawnLimitReached)
        {
            Debug.Log("CornbagSpawner: Spawn limit reached. No more bags will spawn.");
            return;
        }

        Invoke(nameof(SpawnBag), spawnDelay);
    }
}