using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;

public class GrabLog : MonoBehaviour
{

    [SerializeField] private HandGrabInteractor left;
    [SerializeField] private HandGrabInteractor right;
    [SerializeField] private TouchHandGrabInteractor leftTouch;
    [SerializeField] private TouchHandGrabInteractor rightTouch;

    public string GetGrab()
    {
        if (left.IsGrabbing && !leftTouch.HasInteractable)
        {
            return $"left,{left.Interactable.name}";
        } 
        else if (!left.IsGrabbing && leftTouch.HasInteractable)
        {
            return $"left,{leftTouch.Interactable.name}";
        }
        else if (right.IsGrabbing && !rightTouch.HasInteractable)
        {
            return $"right,{right.Interactable.name}";
        }
        else if (!right.IsGrabbing && rightTouch.HasInteractable)
        {
            return $"right,{rightTouch.Interactable.name}";
        }
        else
        {
            return null;
        }
    }
}
