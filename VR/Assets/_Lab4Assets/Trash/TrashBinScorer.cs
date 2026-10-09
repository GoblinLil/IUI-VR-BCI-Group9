using System;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UI;

public class TrashBinScorer : MonoBehaviour
{
    private static readonly int State = Animator.StringToHash("State");

    [Header("Setup")]
    public Transform rimCenter;     // point at the center/top of the bin opening
    [Tooltip("This object should have a Trigger collider (e.g., tall cylinder above bin).")]
    public Collider scoreZone;      // auto-filled by Reset if on same object

    [Header("Rules")]
    public float minReleaseSpeed = 1.2f;      // m/s; prevents “drop-ins”
    public float minReleaseDistance = 1.0f;   // meters from rim at release
    public float maxSecondsSinceRelease = 5f; // throw must be recent
    public bool requireDownwardEntry = true;  // must be moving downward when entering

    [Header("State")]
    public int score;

    public Animator indicate;
    public float leaveAnim = .5f;
    float resetStateAfter = float.NegativeInfinity;
    const int STATUS_IDLE = 0;
    const int STATUS_SCORE = 1;
    const int STATUS_REJECT = 2;
    private const int STATUS_UNKNOWN = 3;
    
    bool m_isComplete;

    public bool IsComplete
    {
        get => m_isComplete;
        private set
        {
            m_isComplete = value;
            doneCheckmark.SetActive(value);
            
            if (value)
            {
                OnCompleted?.Invoke();
            }
        }
    } // <-- new property
    public event Action OnCompleted;
    
    private HashSet<TrashItemThrowData> counted = new();

    public Slider progressSlider; // UI slider to show progress
    public GameObject doneCheckmark; // UI checkmark to show task completion
    
    // DO NOT CHANGE
    void Reset() { scoreZone = GetComponent<Collider>(); UpdateCompletion(); }

    void OnEnable()
    {
        UpdateCompletion();
        progressSlider.maxValue = minScore;
    }
    
    void Update()
    {
        if (Time.time > resetStateAfter)
        {
            indicate.SetInteger(State, STATUS_IDLE);
        }
    }

    public Action<bool> OnTrashAcceptReject;
    
    // called when something enters the scorecollider
    void OnTriggerEnter(Collider other)
    {   
        Debug.Log("Something entered the bin");
        // check what the other rigidbody is
        var rb = other.attachedRigidbody;
        if (!rb) return;

        var data = rb.GetComponent<TrashItemThrowData>();
        if (!data || counted.Contains(data))
        {
            // indicate.SetInteger(State, STATUS_UNKNOWN);
            // resetStateAfter = Time.time + leaveAnim / 2;
            return;
        }
        
        float since = Time.time - data.releaseTime;
        float speed = data.releaseVel.magnitude;
        var center = rimCenter ? rimCenter.position : transform.position;
        float dist = Vector3.Distance(data.releasePos, center);
        bool downward = !requireDownwardEntry || Vector3.Dot(rb.linearVelocity.normalized, Vector3.down) > 0.2f;

        if (since <= maxSecondsSinceRelease && speed >= minReleaseSpeed && dist >= minReleaseDistance && downward)
        {
            score++;
            counted.Add(data);
            Debug.Log($"Trash: SCORE #{score} (speed {speed:F1}, dist {dist:F2}, t {since:F1}s)");
            indicate.SetInteger(State, STATUS_SCORE);
            OnTrashAcceptReject?.Invoke(true);
        }
        else
        {
            Debug.Log($"Trash: rejected (speed {speed:F1}, dist {dist:F2}, t {since:F1}s, down {downward})");
            indicate.SetInteger(State, STATUS_REJECT);
            OnTrashAcceptReject?.Invoke(false);
        }
        resetStateAfter = Time.time + leaveAnim;

        UpdateCompletion();
    }

    const int minScore = 2; // minimum score to complete the task
    // check if the task is completed
    // DO NOT CHANGE
    private void UpdateCompletion()
    {
        // True only if at least 2 valid scores AND nothing removed (still 2+ inside)
        IsComplete = score >= minScore && counted.Count >= minScore;
        progressSlider.value = min(score, counted.Count);
    }

    int min(int a, int b)
    {
        if (a < b) return a;
        return b;
    }
}
