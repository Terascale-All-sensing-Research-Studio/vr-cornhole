using Meta.XR.BuildingBlocks.AIBlocks;
using Oculus.Interaction;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class DataTracker : MonoBehaviour
{
    public string id; // Serialized ID for folder name
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



    //private StreamWriter buttonPressWriter;
    public static string selectionLogger = "";
    //private string previous = "";
    [SerializeField] private InteractableUnityEventWrapper StartButton;
    private bool gameStarted = false;
    private float gameTime = 0f;

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



        //buttonPressWriter = InitButtonPressFile("ButtonPresses.csv");
    }

    void Update()
    {
        long timestampNs = (long)(stopwatch.ElapsedTicks * timestampMultiplier);
        //float gameTime = Time.time;
        //if (StartButton != null)
        //{
        //    if (!gameStarted)
        //    {
        //        gameStarted = true;
        //        gameTime = 0f;
        //    }
        //    else
        //    {
        //        // Increment gameTime by deltaTime each frame
        //        gameTime += Time.deltaTime;
        //    }
        //}
        //if (StartButton != null)
        //{
        if (!gameStarted)
        {
            gameStarted = true;
            gameTime = 0f;
        }
        else
        {
            // Increment gameTime by deltaTime each frame
            gameTime += Time.deltaTime;
        }
        //}

        LogPose(BufferStreamType.Head, headTransform, timestampNs, gameTime);
        LogPose(BufferStreamType.LeftHand, leftHandTransform, timestampNs, gameTime);
        LogPose(BufferStreamType.RightHand, rightHandTransform, timestampNs, gameTime);


       
       

        //if (selectionLogger != "" && previous != selectionLogger)
        //    {
        //        LogButtonPress(timestampNs, gameTime, selectionLogger);
        //        previous = selectionLogger;
        //    }
    }

    //private void LogButtonPress(float timestamp, float gameTime, string buttonName)
    //{
    //    if (buttonPressWriter == null) return;
    //    buttonPressWriter.WriteLine($"{timestamp},\t{gameTime:F4},\t{buttonName},");
    //    buttonPressWriter.Flush();
    //}
    void LogPose(BufferStreamType type, Transform t, long ts, float gt)
    {
        Vector3 p = t.position;
        Vector3 r = t.rotation.eulerAngles;

        string line = $"{ts},{gt:F4},{p.x:F4},{p.y:F4},{p.z:F4},{r.x:F4},{r.y:F4},{r.z:F4},";
        bufferStream.Enqueue(type, line);
    }
    public void LogBagMovement(string bagID, Vector3 position, Vector3 rotation)
    {
        if (Instance == null) return;

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
        //buttonPressWriter?.Close();
    }
}