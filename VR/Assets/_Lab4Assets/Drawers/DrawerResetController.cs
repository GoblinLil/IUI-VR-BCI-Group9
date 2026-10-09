using System;
using UnityEngine;
using UnityEngine.UI;

// DO NOT CHANGE THIS FILE
public class DrawerTaskResetController : MonoBehaviour
{
    [Header("Assign these in Inspector")]
    public DrawerTask drawerTask1;  // drag a drawer here from the inspector
    public DrawerTask drawerTask2;  // drag another drawer       
    public FileStackSpawner stackSpawner; // drag an empty parent with a FileStackSpawner script attached to it

    public Slider progressSlider; // UI slider to show progress
    public GameObject doneCheckmark; // UI checkmark to show task completion
    
    [ContextMenu("Reset Task")]
    public void ResetTask()
    {   
        // reset filestack, then the drawers itself
        if (stackSpawner) stackSpawner.ResetState();
        if (drawerTask1)   drawerTask1.ResetState();
        if (drawerTask2)   drawerTask2.ResetState();
        Debug.Log("[DrawerReset] Task reset.");
        
        OnProgressChanged();
    }
    
    public void OnEnable()
    {
        progressSlider.maxValue = drawerTask1.requiredCount + drawerTask2.requiredCount;

        drawerTask1.OnProgressChanged += OnProgressChanged;
        drawerTask2.OnProgressChanged += OnProgressChanged;
        
        doneCheckmark.SetActive(drawerTask1.IsComplete && drawerTask2.IsComplete);
    }

    void OnProgressChanged()
    {
        progressSlider.value = drawerTask1.current + drawerTask2.current;
        bool isComplete = drawerTask1.IsComplete && drawerTask2.IsComplete;
        doneCheckmark.SetActive(isComplete);
            
            if (isComplete)
            {
                OnCompleted?.Invoke();
            }
    }
    
    public event Action OnCompleted;
}    
