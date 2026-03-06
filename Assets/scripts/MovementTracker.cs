using UnityEngine;

public class MovementTracker : MonoBehaviour
{
    private Rigidbody rb;
    private bool wasKinematic = false;

    private enum BagState { Idle, Grabbed, InFlight }
    private BagState state = BagState.Idle;

    [Tooltip("Tag applied to the cornhole board collider")]
    [SerializeField] private string boardTag = "CornholeBoard";
    [SerializeField] private string floorTag = "Floor";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null || DataTracker.Instance == null) return;

        bool isKinematic = rb.isKinematic;

        // Grabbed — hand takes control, rigidbody goes kinematic
        if (isKinematic && !wasKinematic)
        {
            // Ignore the first frame transition at game start
            if (Time.timeSinceLevelLoad < 1f)
            {
                wasKinematic = isKinematic;
                return;
            }

            state = BagState.Grabbed;
            Debug.Log($"BAG GRABBED: {gameObject.name}");
            DataTracker.Instance.LogBagGrabbed(gameObject.name, transform.position);
        }

        // Released/thrown — rigidbody goes non-kinematic, start trajectory tracking
        if (!isKinematic && wasKinematic && state == BagState.Grabbed)
        {
            state = BagState.InFlight;
            Debug.Log($"BAG RELEASED: {gameObject.name}");
        }

        wasKinematic = isKinematic;

        // Log trajectory every physics tick while held OR in flight
        if (state == BagState.Grabbed || state == BagState.InFlight)
        {
            DataTracker.Instance.LogBagMovement(
                gameObject.name,
                transform.position,
                transform.rotation.eulerAngles
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"COLLISION: {gameObject.name} hit {collision.gameObject.name} | tag: {collision.gameObject.tag} | state: {state}");

        if (state != BagState.InFlight) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 impactPoint = contact.point;
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (collision.gameObject.CompareTag(boardTag))
        {
            state = BagState.Idle;
            Debug.Log($"BAG HIT BOARD: {gameObject.name} | Speed: {impactSpeed:F2} m/s");
            DataTracker.Instance.LogBagImpact(impactPoint, impactSpeed, "BOARD");
        }
        else if (collision.gameObject.CompareTag(floorTag))
        {
            state = BagState.Idle;
            Debug.Log($"BAG HIT FLOOR: {gameObject.name} | Speed: {impactSpeed:F2} m/s");
            DataTracker.Instance.LogBagImpact(impactPoint, impactSpeed, "FLOOR");
        }
    }
}