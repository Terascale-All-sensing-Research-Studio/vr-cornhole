using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour
{
    //rotate player coroutine variables
    public OVRCameraRig player;
    public float rotationAngle = 180f;
    public float delayTime = 2f;
    void Start()
    {
        //set camera rig rotation upon start of scene
        StartCoroutine(RotatePlayerAfterDelay(delayTime, rotationAngle));
    }
    IEnumerator RotatePlayerAfterDelay(float delay, float angle)    //by Yash
    {
        yield return new WaitForSeconds(delay);
        Quaternion newRotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, angle, player.transform.rotation.eulerAngles.z);
        player.transform.rotation = newRotation;
        Debug.Log("Player rotated after " + delay + " seconds.");
    }
}