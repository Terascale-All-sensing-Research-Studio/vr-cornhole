using UnityEngine;

public class MovementTracker : MonoBehaviour
{
    private Rigidbody rb;

    private bool wasKinematic = false;
    private bool isTracking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        if (DataTracker.Instance == null) return;

        bool isKinematic = rb.isKinematic;

        // Detect moment grab starts
        if (isKinematic && !wasKinematic)
        {
            Debug.Log("BAG GRABBED: " + gameObject.name);
            isTracking = true;
        }

        wasKinematic = isKinematic;

        if (isTracking)
        {
            DataTracker.Instance.LogBagMovement(
                gameObject.name,
                transform.position,
                transform.rotation.eulerAngles
            );
        }
    }
}