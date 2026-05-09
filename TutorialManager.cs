using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        Welcome,
        GrabBoth,
        RowBoth,
        Complete
    }

    [Header("References")]
    public Oar leftOar;
    public Oar rightOar;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI repCounterText;
    public BoatController boatController;

    [Header("Highlights")]
    public GameObject leftOarHighlight;
    public GameObject rightOarHighlight;

    [Header("Settings")]
    public float practiceRepsRequired = 5f;
    public float strokeThreshold = 30f;
    public float stepDelay = 1.5f;

    [Header("Audio")]
    public AudioSource tutorialAudioSource;
    public AudioClip stepCompleteClip;
    public AudioClip tutorialCompleteClip;

    private TutorialStep currentStep = TutorialStep.Welcome;
    private int practiceReps = 0;
    private bool _repInProgress = false;
    private bool _leftPulled = false;
    private bool _rightPulled = false;
    private bool _stepLocked = false;

    // ----------------------------------------------------------

    void Start()
    {
        // Highlights glow immediately when scene starts
        if (leftOarHighlight) leftOarHighlight.SetActive(true);
        if (rightOarHighlight) rightOarHighlight.SetActive(true);

        StartTutorial();
    }

    public void StartTutorial()
    {
        practiceReps = 0;
        if (boatController) boatController.enabled = false;
        GoToStep(TutorialStep.Welcome);
    }

    // ----------------------------------------------------------

    void Update()
    {
        if (_stepLocked) return;

        switch (currentStep)
        {
            case TutorialStep.GrabBoth:
                // Wait until both oars are grabbed
                if (leftOar.IsGrabbed && rightOar.IsGrabbed)
                    AdvanceStep();
                break;

            case TutorialStep.RowBoth:
                CheckPracticeRep();
                break;
        }
    }

    // ----------------------------------------------------------

    void CheckPracticeRep()
    {
        if (leftOar.IsGrabbed &&
            leftOar.StrokeVelocity >= strokeThreshold)
            _leftPulled = true;

        if (rightOar.IsGrabbed &&
            rightOar.StrokeVelocity >= strokeThreshold)
            _rightPulled = true;

        if ((_leftPulled || _rightPulled) && !_repInProgress)
            _repInProgress = true;

        bool bothReturned =
            leftOar.StrokeVelocity < strokeThreshold * 0.3f &&
            rightOar.StrokeVelocity < strokeThreshold * 0.3f;

        if (_repInProgress && _leftPulled && _rightPulled && bothReturned)
        {
            _repInProgress = false;
            _leftPulled = false;
            _rightPulled = false;

            practiceReps++;
            repCounterText.text = $"{practiceReps} / {(int)practiceRepsRequired}";
            PlayStepComplete();

            if (practiceReps >= (int)practiceRepsRequired)
                AdvanceStep();
        }
    }

    // ----------------------------------------------------------

    void GoToStep(TutorialStep step)
    {
        currentStep = step;

        if (repCounterText) repCounterText.gameObject.SetActive(false);

        switch (step)
        {
            case TutorialStep.Welcome:
                SetInstruction("Pinch your controllers\nto grab the oars.");
                StartCoroutine(AutoAdvanceAfter(3f));
                break;

            case TutorialStep.GrabBoth:
                SetInstruction("Grab both oars.\nKeep controllers pinched.");
                break;

            case TutorialStep.RowBoth:
                SetInstruction("Row 5 strokes.\nYou'll feel a buzz for good form!");
                if (repCounterText)
                {
                    repCounterText.gameObject.SetActive(true);
                    repCounterText.text = $"0 / {(int)practiceRepsRequired}";
                }
                practiceReps = 0;
                _repInProgress = false;
                _leftPulled = false;
                _rightPulled = false;
                break;

            case TutorialStep.Complete:
                SetInstruction("You're ready. Let's go!");
                PlayClip(tutorialCompleteClip);
                StartCoroutine(LaunchSessionAfter(3f));
                break;
        }
    }

    // ----------------------------------------------------------

    void AdvanceStep()
    {
        StartCoroutine(AdvanceWithDelay());
    }

    IEnumerator AdvanceWithDelay()
    {
        _stepLocked = true;
        PlayStepComplete();
        yield return new WaitForSeconds(stepDelay);
        _stepLocked = false;
        GoToStep(currentStep + 1);
    }

    void SetInstruction(string text)
    {
        if (instructionText) instructionText.text = text;
    }

    void PlayStepComplete() { PlayClip(stepCompleteClip); }

    void PlayClip(AudioClip clip)
    {
        if (tutorialAudioSource != null && clip != null)
            tutorialAudioSource.PlayOneShot(clip);
    }

    IEnumerator AutoAdvanceAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (currentStep == TutorialStep.Welcome)
            AdvanceStep();
    }

    IEnumerator LaunchSessionAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        OnTutorialComplete();
    }

    void OnTutorialComplete()
    {
        if (leftOarHighlight) leftOarHighlight.SetActive(false);
        if (rightOarHighlight) rightOarHighlight.SetActive(false);
        if (boatController) boatController.enabled = true;

        FindObjectOfType<GoalManager>().StartSession();

        gameObject.SetActive(false);
    }
}