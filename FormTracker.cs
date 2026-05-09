using UnityEngine;
using TMPro;
using System.Collections;

public class FormTracker : MonoBehaviour
{
    [Header("References")]
    public Oar leftOar;
    public Oar rightOar;

    [Header("HUD")]
    public TextMeshProUGUI formText;
    public TextMeshProUGUI formDetailText;

    [Header("Thresholds")]
    public float strokeThreshold = 30f;
    public float unevenThreshold = 20f;
    public float tooFastThreshold = 150f;
    public float perfectThreshold = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip goodFormClip;
    public AudioClip perfectFormClip;

    [Header("Haptics")]
    [Range(0f, 1f)] public float goodHapticAmplitude = 0.3f;
    [Range(0f, 1f)] public float perfectHapticAmplitude = 0.7f;
    public float hapticDuration = 0.2f;

    [Header("Oar Blade Tips")]
    public Transform leftBladeTip;
    public Transform rightBladeTip;

    [Header("Particles — Good Form")]
    public ParticleSystem leftGoodSplash;
    public ParticleSystem rightGoodSplash;

    [Header("Particles — Perfect Form")]
    public ParticleSystem leftPerfectBurst;
    public ParticleSystem rightPerfectBurst;

    [Header("Water Glow")]
    public Renderer waterRenderer;
    public Color waterNormalColor = new Color(0.10f, 0.35f, 0.55f);
    public Color waterGoodColor = new Color(0.10f, 0.75f, 0.55f);
    public Color waterPerfectColor = new Color(0.00f, 1.00f, 0.70f);
    public float waterColorDuration = 1.2f;
    public string waterColorProperty = "_BaseColor";

    [Header("Display")]
    public float feedbackDisplayDuration = 2.5f;

    private float _leftPeakVelocity = 0f;
    private float _rightPeakVelocity = 0f;
    private bool _trackingActive = false;
    private int _goodStreak = 0;
    private Coroutine _clearFeedbackCoroutine;
    private Coroutine _waterColorCoroutine;

    public enum FormRating { Perfect, Good, Uneven, TooFast }
    public FormRating LastRating { get; private set; }

    // ----------------------------------------------------------

    public void StartTracking()
    {
        _trackingActive = true;
        _leftPeakVelocity = 0f;
        _rightPeakVelocity = 0f;
        ClearDisplay();
    }

    public void StopTracking()
    {
        _trackingActive = false;
        _goodStreak = 0;
        ClearDisplay();
    }

    // ----------------------------------------------------------

    void Update()
    {
        if (!_trackingActive) return;

        if (leftOar != null && leftOar.IsGrabbed &&
            leftOar.StrokeVelocity >= strokeThreshold)
            _leftPeakVelocity = Mathf.Max(
                _leftPeakVelocity, leftOar.StrokeVelocity);

        if (rightOar != null && rightOar.IsGrabbed &&
            rightOar.StrokeVelocity >= strokeThreshold)
            _rightPeakVelocity = Mathf.Max(
                _rightPeakVelocity, rightOar.StrokeVelocity);
    }

    // ----------------------------------------------------------

    public FormRating EvaluateRep()
    {
        float diff = Mathf.Abs(_leftPeakVelocity - _rightPeakVelocity);
        float maxPeak = Mathf.Max(_leftPeakVelocity, _rightPeakVelocity);

        FormRating rating;

        if (maxPeak >= tooFastThreshold)
            rating = FormRating.TooFast;
        else if (diff >= unevenThreshold)
            rating = FormRating.Uneven;
        else if (diff <= perfectThreshold)
            rating = FormRating.Perfect;
        else
            rating = FormRating.Good;

        if (rating == FormRating.Perfect || rating == FormRating.Good)
            _goodStreak++;
        else
            _goodStreak = 0;

        LastRating = rating;
        DisplayFeedback(rating);

        _leftPeakVelocity = 0f;
        _rightPeakVelocity = 0f;

        return rating;
    }

    // ----------------------------------------------------------

    void DisplayFeedback(FormRating rating)
    {
        if (_clearFeedbackCoroutine != null)
            StopCoroutine(_clearFeedbackCoroutine);

        switch (rating)
        {
            case FormRating.Perfect:
                FireParticles(leftPerfectBurst, leftBladeTip);
                FireParticles(rightPerfectBurst, rightBladeTip);
                SetWaterColor(waterPerfectColor);
                PlayClip(perfectFormClip);
                TriggerHaptics(perfectHapticAmplitude, hapticDuration);
                SetFormText(_goodStreak >= 5
                    ? $"{_goodStreak} in a row!"
                    : "Perfect!", new Color(0.08f, 0.85f, 0.60f));
                SetDetailText("Arms perfectly balanced");
                _clearFeedbackCoroutine =
                    StartCoroutine(ClearAfter(feedbackDisplayDuration));
                break;

            case FormRating.Good:
                FireParticles(leftGoodSplash, leftBladeTip);
                FireParticles(rightGoodSplash, rightBladeTip);
                SetWaterColor(waterGoodColor);
                PlayClip(goodFormClip);
                TriggerHaptics(goodHapticAmplitude, hapticDuration);
                SetFormText("Good form", new Color(0.11f, 0.62f, 0.46f));
                SetDetailText(_goodStreak >= 3
                    ? $"{_goodStreak} in a row!"
                    : "Keep it up");
                _clearFeedbackCoroutine =
                    StartCoroutine(ClearAfter(feedbackDisplayDuration));
                break;

            case FormRating.Uneven:
                string side = _leftPeakVelocity > _rightPeakVelocity
                    ? "Left" : "Right";
                SetFormText("Uneven", new Color(0.73f, 0.46f, 0.09f));
                SetDetailText($"{side} arm pulling harder — match both arms");
                _clearFeedbackCoroutine =
                    StartCoroutine(ClearAfter(feedbackDisplayDuration));
                break;

            case FormRating.TooFast:
                SetFormText("Slow down", new Color(0.89f, 0.29f, 0.29f));
                SetDetailText("Squeeze your shoulder blades");
                _clearFeedbackCoroutine =
                    StartCoroutine(ClearAfter(feedbackDisplayDuration));
                break;
        }
    }

    // ----------------------------------------------------------

    void FireParticles(ParticleSystem ps, Transform tip)
    {
        if (ps == null) return;

        if (!ps.gameObject.activeInHierarchy)
            ps.gameObject.SetActive(true);

        if (tip != null)
        {
            ps.transform.position = tip.position;
            ps.transform.rotation = tip.rotation;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }

    // ----------------------------------------------------------

    void SetWaterColor(Color target)
    {
        if (_waterColorCoroutine != null)
            StopCoroutine(_waterColorCoroutine);
        _waterColorCoroutine = StartCoroutine(FlashWaterColor(target));
    }

    IEnumerator FlashWaterColor(Color target)
    {
        if (waterRenderer == null) yield break;

        float elapsed = 0f;
        float halfTime = waterColorDuration * 0.5f;

        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            waterRenderer.material.SetColor(waterColorProperty,
                Color.Lerp(waterNormalColor, target, elapsed / halfTime));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            waterRenderer.material.SetColor(waterColorProperty,
                Color.Lerp(target, waterNormalColor, elapsed / halfTime));
            yield return null;
        }

        waterRenderer.material.SetColor(
            waterColorProperty, waterNormalColor);
    }

    // ----------------------------------------------------------

    void TriggerHaptics(float amplitude, float duration)
    {
        OVRInput.SetControllerVibration(
            amplitude, amplitude, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(
            amplitude, amplitude, OVRInput.Controller.RTouch);
        StartCoroutine(StopHapticsAfter(duration));
    }

    IEnumerator StopHapticsAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ----------------------------------------------------------

    void SetFormText(string text, Color color)
    {
        if (!formText) return;
        formText.text = text;
        formText.color = color;
    }

    void SetDetailText(string text)
    {
        if (!formDetailText) return;
        formDetailText.text = text;
    }

    IEnumerator ClearAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearDisplay();
    }

    void ClearDisplay()
    {
        SetFormText("", Color.white);
        SetDetailText("");
    }
}