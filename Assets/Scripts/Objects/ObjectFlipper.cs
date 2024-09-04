using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles smooth interpolation of scale values from current value to a goal.
/// Creates a flattening before swapping effect (useful especially for the player).
/// Should be located ONLY on sprite child objects of game objects with movement functionality (to prevent impacting mechanics).
/// </summary>
public class ObjectFlipper : MonoBehaviour
{
    [SerializeField, Tooltip("Scale value threshold at which actual scale will snap to goal scale")]
    private float _snappingThreshold = 0.01f;
    [SerializeField, Tooltip("'snappiness' of scale value changes towards goal value")]
    private float _changeSharpness = 1f;

    private float _goalScaleX;

    // Start is called before the first frame update
    void Start()
    {
        _goalScaleX = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        // Snap to goal if close enough
        if (Mathf.Abs(transform.localScale.x - _goalScaleX) < _snappingThreshold)
        {
            Vector3 snapScale = transform.localScale;
            snapScale.x = _goalScaleX;
            transform.localScale = snapScale;        }
        else // smoothly lerp towards goal
        {
            float lerpScaleX = Mathf.Lerp(transform.localScale.x, _goalScaleX, 1f - Mathf.Exp(-_changeSharpness * Time.deltaTime));
            Vector3 lerpScale = transform.localScale;
            lerpScale.x = lerpScaleX;
            transform.localScale = lerpScale;
        }
    }

    public void SetScaleX(float scaleX)
    {
        _goalScaleX = scaleX;
    }
}
