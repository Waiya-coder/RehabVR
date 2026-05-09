using UnityEngine;

public class SeatPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRig;
    public Transform playerSeat; // child of boat

    private Vector3 offset;
    private Quaternion rotationOffset;

    void Start()
    {
        // Snap to seat at start
        cameraRig.position = playerSeat.position;
        cameraRig.rotation = playerSeat.rotation;

        // Store offset between camera and seat
        offset = cameraRig.position - playerSeat.position;
        rotationOffset = Quaternion.Inverse(playerSeat.rotation) * cameraRig.rotation;
    }

    void LateUpdate()
    {
        // Follow the seat as the boat moves
        cameraRig.position = playerSeat.position + offset;
        cameraRig.rotation = playerSeat.rotation * rotationOffset;
    }
}