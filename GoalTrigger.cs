using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public GoalManager goalManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat"))
            goalManager.OnGoalReached();
    }
}