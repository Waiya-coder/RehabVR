using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Oars")]
    public Oar leftOar;
    public Oar rightOar;

    [Header("Physics")]
    public Rigidbody rb;
    public float forceMultiplier = 50f;
    public float steeringMultiplier = 3f;
    public float waterDrag = 2f;
    public float maxAngularSpeed = 0.8f;
    public float strokeThreshold = 30f;

    [Header("Steering Dead Zone")]
    [Tooltip("Minimum stroke difference before steering activates")]
    public float steerDeadZone = 15f;

    [Header("Audio")]
    public AudioSource boatAudioSource;
    public float minSpeedForSound = 0.3f;
    public float maxVolume = 1f;   // crank this up in Inspector
    public float speedForMaxVol = 3f;   // reach max volume at this speed

    private bool _frozen = false;

    // ----------------------------------------------------------

    void Start()
    {
        if (boatAudioSource != null)
        {
            boatAudioSource.loop = true;
            boatAudioSource.volume = 0f;
            boatAudioSource.Play();
        }
    }
  

    // ----------------------------------------------------------

    void FixedUpdate()
    {
        if (_frozen) return;

        // 1. Water drag
        rb.AddForce(-rb.linearVelocity * waterDrag, ForceMode.Force);

        // 2. Cap angular velocity
        if (rb.angularVelocity.magnitude > maxAngularSpeed)
            rb.angularVelocity =
                rb.angularVelocity.normalized * maxAngularSpeed;

        // 3. Read strokes
        float leftForce = (leftOar.IsGrabbed &&
            leftOar.StrokeVelocity >= strokeThreshold)
            ? leftOar.StrokeVelocity : 0f;
        float rightForce = (rightOar.IsGrabbed &&
            rightOar.StrokeVelocity >= strokeThreshold)
            ? rightOar.StrokeVelocity : 0f;

        // 4. Forward push
        float forwardPush = (leftForce + rightForce) * forceMultiplier;
        rb.AddForce(transform.forward * forwardPush, ForceMode.Force);

        // 5. Steering
        float steerDiff = leftForce - rightForce;
        if (Mathf.Abs(steerDiff) > steerDeadZone)
            rb.AddTorque(Vector3.up * steerDiff * steeringMultiplier,
                ForceMode.Force);

        // 6. Audio — scales up to maxVolume much more aggressively
        if (boatAudioSource != null)
        {
            float speed = rb.linearVelocity.magnitude;
            float targetVolume = speed > minSpeedForSound
                ? Mathf.Clamp01(speed / speedForMaxVol) * maxVolume
                : 0f;
            boatAudioSource.volume = Mathf.MoveTowards(
                boatAudioSource.volume, targetVolume,
                Time.fixedDeltaTime * 4f); // faster fade in/out
        }
    }

    // ----------------------------------------------------------

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Goal")) return;

        _frozen = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (boatAudioSource != null)
            boatAudioSource.volume = 0f;
    }
}