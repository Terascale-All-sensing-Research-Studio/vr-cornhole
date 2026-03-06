using UnityEngine;
using Oculus.Interaction;

public class StartScreen : MonoBehaviour
{

    [SerializeField] private InteractableUnityEventWrapper starteventWrapper;

    [SerializeField] private GameObject barrier; // walls or starting area
    [SerializeField] private GameObject gameArea; // main gameplay area

    public bool start = false;

    private void OnEnable()
    {
        if (starteventWrapper != null)
        {
            starteventWrapper.WhenSelect.AddListener(OnButtonPoked);
        }
    }

    private void OnDisable()
    {
        if (starteventWrapper != null)
        {
            starteventWrapper.WhenSelect.RemoveListener(OnButtonPoked);
        }
    }

    private void Start()
    {

        if (barrier != null)
            barrier.SetActive(true); // show starting walls
    }

    private void OnButtonPoked()
    {
        Debug.Log("Button poked! Start triggered.");

        if (barrier != null)
            barrier.SetActive(false); // remove starting area

  
        // Hide the button itself
        if (starteventWrapper != null)
            starteventWrapper.gameObject.SetActive(false); // disable the wrapper's GameObject

        start = true;
    }
}
