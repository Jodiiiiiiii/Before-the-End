using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles game level timers for things such as shuffling quantum objects and synced 2-frame animations.
/// There should only be one of these in a scene at once, attached to the main panel
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Quantum Timer")]
    [SerializeField, Tooltip("Period of timer for calling the function to shuffle quantum objects.")]
    private static float _quantumTimerPeriod = 0.5f;

    private static float _quantumTimer;

    // Start is called before the first frame update
    void Start()
    {
        _quantumTimer = _quantumTimerPeriod;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle quantum timer
        if (_quantumTimer < 0)
        {
            // shuffle and reset timer
            ObjectState.ShuffleHiddenQuantumObjects();
            _quantumTimer = _quantumTimerPeriod;
        }
        else
            _quantumTimer -= Time.deltaTime;
    }
}
