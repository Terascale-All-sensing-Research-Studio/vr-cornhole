using Meta.XR.BuildingBlocks.AIBlocks;
using Oculus.Interaction;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class DataTracker : MonoBehaviour
{
    public string id;
    public OVRHand leftHand;
    public OVRHand rightHand;
    public GameObject head;
    [SerializeField] private GrabLog grabLog;

    private BufferStream bufferStream;
    private Stopwatch stopwatch;
    private double timestampMultiplier;

    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private Transform headTransform;

    public static DataTracker Instance;
    public static MovementTracker MovementTracker;

    public static string selectionLogger = "";

    [SerializeField] private InteractableUnityEventWrapper StartButton;
    private bool gameStarted = false;
    private float gameTime = 0f;

    // Holds the grab half of the row until impact completes it
    private string pendingBagRow = null;

    void Start()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        bufferStream = new BufferStream();

        string path = "C:\\Users\\teras\\OneDrive\\Documents\\Cornhole_test\\test";
        bufferStream.CreateDirectory(path);

        leftHandTransform = leftHand.transform;
        rightHandTransform = rightHand.transform;
        headTransform = head.transform;

        stopwatch = Stopwatch.StartNew();
        timestampMultiplier = 1e9 / Stopwatch.Frequency;
    }

    void Update()
    {
        long timestampNs = (long)(stopwatch.ElapsedTicks * timestampMultiplier);

        if (!gameStarted)
        {
            gameStarted = true;
            gameTime = 0f;
        }
        else
        {
            gameTime += Time.deltaTime;
        }

        LogPose(BufferStreamType.Head, headTransform, timestampNs, gameTime);
        LogPose(BufferStreamType.LeftHand, leftHandTransform, timestampNs, gameTime);
        LogPose(BufferStreamType.RightHand, rightHandTransform, timestampNs, gameTime);
    }

    void LogPose(BufferStreamType type, Transform t, long ts, float gt)
    {
        Vector3 p = t.position;
        Vector3 r = t.rotation.eulerAngles;
        string line = $"{ts},{gt:F4},{p.x:F4},{p.y:F4},{p.z:F4},{r.x:F4},{r.y:F4},{r.z:F4},";
        bufferStream.Enqueue(type, line);
    }

    // Called by MovementTracker when bag is grabbed
    public void LogBagGrabbed(string bagID, Vector3 position)
    {
        long timestampNs = (long)(stopwatch.ElapsedTicks * timestampMultiplier);

        // Store grab data — row stays open until impact
        pendingBagRow =
            $"{timestampNs},{gameTime:F4},{bagID}," +
            $"{position.x:F4},{position.y:F4},{position.z:F4}";
    }

    // Called by MovementTracker when bag hits the board
    public void LogBagImpact(Vector3 impactPoint, float impactSpeed, string destination)
    {
        if (pendingBagRow == null)
        {
            UnityEngine.Debug.LogWarning("LogBagImpact called but no pending grab row.");
            return;
        }

        long timestampNs = (long)(stopwatch.ElapsedTicks * timestampMultiplier);

        string completedRow =
            pendingBagRow +
            $",{timestampNs},{gameTime:F4},{destination}," +
            $"{impactPoint.x:F4},{impactPoint.y:F4},{impactPoint.z:F4}," +
            $"{impactSpeed:F4},";

        bufferStream.Enqueue(BufferStreamType.HandGrab, completedRow);
        pendingBagRow = null;
    }
    // Called every physics tick while bag is in flight
    public void LogBagMovement(string bagID, Vector3 position, Vector3 rotation)
    {
        long timestampNs = (long)(stopwatch.ElapsedTicks * timestampMultiplier);

        string line =
            $"{timestampNs},{gameTime:F4},{bagID}," +
            $"{position.x:F4},{position.y:F4},{position.z:F4}," +
            $"{rotation.x:F4},{rotation.y:F4},{rotation.z:F4},";

        bufferStream.Enqueue(BufferStreamType.BagMovement, line);
    }
    void LogEyeHit(long ts, float gt, string name, Vector3 pos)
    {
        string line = $"{ts},{gt:F4},{name},{pos.x:F4},{pos.y:F4},{pos.z:F4},";
        bufferStream.Enqueue(BufferStreamType.EyeHit, line);
    }

    void LogGrab(long ts, float gt, string grabbedObject)
    {
        string line = $"{ts},{gt:F4},{grabbedObject}";
        bufferStream.Enqueue(BufferStreamType.HandGrab, line);
    }

    void OnApplicationQuit()
    {
        bufferStream.StopAndFlush();
    }
}