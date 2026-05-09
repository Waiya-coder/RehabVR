using UnityEngine;
using TMPro;
using System.Collections;

public class GoalManager : MonoBehaviour
{
    [Header("References")]
    public Transform boat;
    public Oar leftOar;
    public Oar rightOar;

    [Header("HUD")]
    public TextMeshProUGUI repCounterText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI timerText;

    [Header("Level Settings")]
    public string currentLevel = "Level1";
    public bool hasTimer = false;
    public float timeLimit = 90f;

    [Header("Stroke Settings")]
    public float strokeThreshold = 30f;

    [Header("Form")]
    public FormTracker formTracker;

    [Header("Celebration")]
    public GoalCelebration[] goalCelebrations;
    public Light[] celebrationLights;
    public float celebrationLightIntensity = 5f;

    [Header("Wind — Level 2 only")]
    public WindController windController;

    [Header("Scene Management")]
    public SceneController sceneController;

    [Header("Level Complete Audio")]
    public AudioSource levelCompleteAudioSource;
    public AudioClip levelCompleteClip;

    private Transform[] _lampposts;
    private int currentReps = 0;
    private bool sessionActive = false;
    private bool goalReached = false;
    private bool _repInProgress = false;
    private bool _leftPulledThisRep = false;
    private bool _rightPulledThisRep = false;
    private float _timeRemaining = 0f;

    // ----------------------------------------------------------

    void Awake()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag("Lamp");
        _lampposts = new Transform[found.Length];
        for (int i = 0; i < found.Length; i++)
            _lampposts[i] = found[i].transform;
    }

    // ----------------------------------------------------------

    public void StartSession()
    {
        currentReps = 0;
        sessionActive = true;
        goalReached = false;
        _repInProgress = false;
        _leftPulledThisRep = false;
        _rightPulledThisRep = false;
        _timeRemaining = timeLimit;

        // Re-enable HUD
        if (repCounterText) repCounterText.gameObject.SetActive(true);
        if (instructionText) instructionText.gameObject.SetActive(true);
        if (timerText) timerText.gameObject.SetActive(hasTimer);

        // Start form tracking
        if (formTracker) formTracker.StartTracking();

        UpdateRepHUD();

        if (instructionText)
            instructionText.text = currentLevel == "Level2"
                ? "The lake is in turmoil!\nRow hard — keep your form!"
                : "Row to the light.";

        Debug.Log($"[GoalManager] StartSession called — Level: {currentLevel}");
    }

    // ----------------------------------------------------------

    void Update()
    {
        if (!sessionActive || goalReached) return;

        UpdateDistanceHUD();
        TrackRep();

        if (hasTimer) UpdateTimer();
    }

    // ----------------------------------------------------------

    void UpdateTimer()
    {
        _timeRemaining -= Time.deltaTime;

        if (timerText)
        {
            int mins = Mathf.FloorToInt(_timeRemaining / 60f);
            int secs = Mathf.FloorToInt(_timeRemaining % 60f);
            timerText.text = $"{mins:0}:{secs:00}";
            timerText.color = _timeRemaining <= 30f
                ? Color.red : Color.white;
        }

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            OnTimerExpired();
        }
    }

    void OnTimerExpired()
    {
        sessionActive = false;
        if (formTracker) formTracker.StopTracking();
        if (windController) windController.StopWind();
        if (instructionText) instructionText.text = "Time's up!";

        if (levelCompleteAudioSource != null && levelCompleteClip != null)
        {
            levelCompleteAudioSource.PlayOneShot(levelCompleteClip);
            StartCoroutine(DelayedRestartPrompt(levelCompleteClip.length));
        }
        else
        {
            if (sceneController) sceneController.ShowRestartPrompt();
        }
    }

    // ----------------------------------------------------------

    void UpdateDistanceHUD()
    {
        if (!distanceText || !boat ||
            _lampposts == null || _lampposts.Length == 0) return;

        float nearest = float.MaxValue;
        foreach (var lp in _lampposts)
        {
            if (lp == null) continue;
            float d = Vector3.Distance(boat.position, lp.position);
            if (d < nearest) nearest = d;
        }

        distanceText.text = $"{Mathf.RoundToInt(nearest)}m";
    }

    // ----------------------------------------------------------

    void TrackRep()
    {
        Debug.Log($"[GoalManager] TrackRep — sessionActive: {sessionActive} leftGrabbed: {leftOar.IsGrabbed} leftVel: {leftOar.StrokeVelocity} rightGrabbed: {rightOar.IsGrabbed} rightVel: {rightOar.StrokeVelocity} repInProgress: {_repInProgress} leftPulled: {_leftPulledThisRep} rightPulled: {_rightPulledThisRep}");

        if (leftOar.IsGrabbed &&
            leftOar.StrokeVelocity >= strokeThreshold)
            _leftPulledThisRep = true;

        if (rightOar.IsGrabbed &&
            rightOar.StrokeVelocity >= strokeThreshold)
            _rightPulledThisRep = true;

        if ((_leftPulledThisRep || _rightPulledThisRep) && !_repInProgress)
            _repInProgress = true;

        bool bothReturned =
            leftOar.StrokeVelocity < strokeThreshold * 0.3f &&
            rightOar.StrokeVelocity < strokeThreshold * 0.3f;

        Debug.Log($"[GoalManager] bothReturned: {bothReturned}");

        if (_repInProgress && _leftPulledThisRep &&
            _rightPulledThisRep && bothReturned)
            ConfirmRep();
    }

    void ConfirmRep()
    {
        currentReps++;
        if (formTracker) formTracker.EvaluateRep();
        UpdateRepHUD();

        _repInProgress = false;
        _leftPulledThisRep = false;
        _rightPulledThisRep = false;

        if (instructionText)
            instructionText.text = currentLevel == "Level2"
                ? currentReps % 3 == 0
                    ? "Push harder — don't let the wind win!"
                    : "Keep rowing hard!"
                : "Keep rowing!";

        Debug.Log($"[GoalManager] Rep confirmed — total: {currentReps}");
    }

    void UpdateRepHUD()
    {
        if (repCounterText)
            repCounterText.text = $"{currentReps} reps";
    }

    // ----------------------------------------------------------

    public void OnGoalReached()
    {
        if (goalReached) return;

        goalReached = true;
        sessionActive = false;

        if (formTracker) formTracker.StopTracking();
        if (windController) windController.StopWind();

        if (repCounterText)
            repCounterText.text = $"{currentReps} reps";

        if (instructionText)
            instructionText.text = currentLevel == "Level2"
                ? "Incredible — you beat the storm!"
                : "Amazing work!\nGet ready — the next level is harder.";

        foreach (var c in goalCelebrations)
            if (c != null) c.Celebrate();

        foreach (var l in celebrationLights)
            if (l != null) l.intensity = celebrationLightIntensity;

        if (levelCompleteAudioSource != null && levelCompleteClip != null)
        {
            levelCompleteAudioSource.PlayOneShot(levelCompleteClip);
            StartCoroutine(DelayedSceneTransition(levelCompleteClip.length));
        }
        else
        {
            if (sceneController) sceneController.OnLevelComplete();
        }
    }
    // ----------------------------------------------------------

    IEnumerator DelayedSceneTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sceneController) sceneController.OnLevelComplete();
    }

    IEnumerator DelayedRestartPrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sceneController) sceneController.ShowRestartPrompt();
    }
}