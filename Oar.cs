using UnityEngine;
using Oculus.Interaction;

public class Oar : MonoBehaviour
{
    public Grabbable grabbable;
    public bool IsGrabbed => grabbable != null && grabbable.SelectingPointsCount > 0;

    [Header("Stroke")]
    [Tooltip("Set to -1 or 1 depending on which way your oar rotates on the pull stroke")]
    public float pullDirection = -1f;

    [Header("Audio")]
    public AudioSource splashAudioSource;
    public float strokeThreshold = 30f;
    public float splashCooldown = 0.4f;

    private float lastAngle;
    private bool wasGrabbed = false;
    private float lastSplashTime = -999f;
    private bool strokeFired = false;

    public float StrokeVelocity { get; private set; }

    void FixedUpdate()
    {
        float angle = GetWrappedAngle();

        if (!IsGrabbed)
        {
            lastAngle = angle;
            wasGrabbed = false;
            StrokeVelocity = 0f;
            strokeFired = false;
            return;
        }

        if (!wasGrabbed)
        {
            lastAngle = angle;
            wasGrabbed = true;
            StrokeVelocity = 0f;
            strokeFired = false;
            return;
        }

        float delta = Mathf.DeltaAngle(lastAngle, angle);
        float rawVelocity = delta / Time.fixedDeltaTime;

        // Only count the pull direction; return stroke gives 0 force
        bool isPulling = (delta * pullDirection) > 0f;
        StrokeVelocity = isPulling ? Mathf.Min(Mathf.Abs(rawVelocity), 180f) : 0f;

        // Splash on pull stroke crossing threshold
        if (StrokeVelocity >= strokeThreshold && !strokeFired
            && Time.time - lastSplashTime >= splashCooldown)
        {
            PlaySplash();
            strokeFired = true;
            lastSplashTime = Time.time;
        }

        // Reset once oar slows so next stroke can fire again
        if (StrokeVelocity < strokeThreshold * 0.3f)
            strokeFired = false;

        lastAngle = angle;
    }

    void PlaySplash()
    {
        if (splashAudioSource == null) return;
        splashAudioSource.pitch = Random.Range(0.9f, 1.1f);
        splashAudioSource.Play();
    }

    private float GetWrappedAngle()
    {
        float a = transform.localEulerAngles.y;
        return a > 180f ? a - 360f : a;
    }
}


