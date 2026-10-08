using System;
using UnityEngine;
using UnityEngine.UI;

/**
This class handles the logic of the cleaning task

You can make changes in this file
*/
public class CleaningTask : MonoBehaviour
{   
    [Header("You can change this file, just not these pre-set parameters")]
    [Header("Drag colliders here")]
    public Collider[] targets;       // Drag grid colliders manually

    [Header("Filter (assign the sponge's Rigidbody)")]
    public Rigidbody spongeRigidbody; 

    [Header("State")]
    public bool cleaningTask;        // True when all zones touched
    private bool m_isComplete;
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
    }
    public event Action OnCompleted;
            
    public Slider progressSlider; // UI slider to show progress
    public GameObject doneCheckmark; // UI checkmark to show task completion
    
    // Internal fields
    private bool[] touched;
    private int touchedCount;

    void progress(int count)
    {
        progressSlider.value = count;
    }

    private int maxVal;
    
    // DO NOT CHANGE THIS METHOD
    void Start()
    {

        // initialize array keeping track of progress
        int n = (targets != null) ? targets.Length : 0;
        touched = new bool[n];
        touchedCount = 0;
        maxVal = n;
        progressSlider.maxValue = n;
        progress(touchedCount);

        // no zones means already complete
        cleaningTask = (n == 0);
    }

    public void onReset()
    {
        progress(0);
    }

    // This method is called when a trigger collider is touched
    void OnTriggerEnter(Collider other)
    {
        if (cleaningTask || targets == null) return;

        // Only count when the assigned sponge Rigidbody touches the zone
        if (spongeRigidbody != null && other.attachedRigidbody != spongeRigidbody)
            return;

        // Loop over the targets, to see if this collision is a new one
        for (int i = 0; i < targets.Length; i++)
        {
            if (!touched[i] && other == targets[i])
            {
                touched[i] = true;
                touchedCount++;
                progress(touchedCount);
                Debug.Log("Touched a cleaning spot");

                // disable particles on it
                if (other.TryGetComponent(out ParticleSystem ps))
                {
                    ps.Stop();
                }
                
                if (touchedCount >= targets.Length)
                {
                    cleaningTask = true;
                    IsComplete = true;
                    Debug.Log("Cleaning task COMPLETE: all zones touched.");
                }
                break;
            }
        }
    }

}
