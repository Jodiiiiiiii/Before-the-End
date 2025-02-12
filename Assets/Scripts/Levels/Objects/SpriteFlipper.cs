using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles smooth interpolation of scale values from current value to a goal.
/// Creates a flattening before swapping effect (useful especially for the player).
/// Should be located ONLY on sprite child objects of game objects with movement functionality (to prevent impacting mechanics).
/// </summary>
public class SpriteFlipper : MonoBehaviour
{
    [SerializeField, Tooltip("Scale value threshold at which actual scale will snap to goal scale")]
    private float _snappingThreshold = 0.01f;
    [SerializeField, Tooltip("'snappiness' of scale value changes towards goal value")]
    private float _changeSharpness = 1f;

    private int _goalScaleX;
    private int _goalScaleY;

    // Start is called before the first frame update
    void Start()
    {
        _goalScaleX = Mathf.RoundToInt(transform.localScale.x);
        _goalScaleY = Mathf.RoundToInt(transform.localScale.y);
    }

    // Update is called once per frame
    void Update()
    {
        // X SCALING
        // Snap to goalScaleX if close enough
        if (Mathf.Abs(transform.localScale.x - _goalScaleX) < _snappingThreshold)
        {
            Vector3 snapScale = transform.localScale;
            snapScale.x = _goalScaleX;
            transform.localScale = snapScale;
        }
        else // smoothly lerp towards goalScaleX
        {
            float lerpScaleX = Mathf.Lerp(transform.localScale.x, _goalScaleX, 1f - Mathf.Exp(-_changeSharpness * Time.deltaTime));
            Vector3 lerpScale = transform.localScale;
            lerpScale.x = lerpScaleX;
            transform.localScale = lerpScale;
        }

        // Y SCALING
        // Snap to goalScaleY if close enough
        if (Mathf.Abs(transform.localScale.y - _goalScaleY) < _snappingThreshold)
        {
            Vector3 snapScale = transform.localScale;
            snapScale.y = _goalScaleY;
            transform.localScale = snapScale;
        }
        else // smoothly lerp towards goalScaleY
        {
            float lerpScaleY = Mathf.Lerp(transform.localScale.y, _goalScaleY, 1f - Mathf.Exp(-_changeSharpness * Time.deltaTime));
            Vector3 lerpScale = transform.localScale;
            lerpScale.y = lerpScaleY;
            transform.localScale = lerpScale;
        }
    }

    /// <summary>
    /// enforces scaleX of only -1, 1, or 0.
    /// </summary>
    public void SetScaleX(int scaleX)
    {
        if (scaleX == -1 || scaleX == 1 || scaleX == 0)
            _goalScaleX = scaleX;
        else
            throw new System.Exception("ObjectFlipper can ONLY be used to set scale values to -1, 1, or 0.");
    }

    /// <summary>
    /// Returns goal scaleX as an integer.
    /// </summary>
    public int GetGoalScaleX()
    {
        return _goalScaleX;
    }

    /// <summary>
    /// returns current scaleX as a float.
    /// </summary>
    public float GetCurrentScaleX()
    {
        return transform.localScale.x;
    }

    /// <summary>
    /// enforces scaleX of only -1, 1, or 0.
    /// </summary>
    public void SetScaleY(int scaleY)
    {
        if (scaleY == -1 || scaleY == 1 || scaleY == 0)
            _goalScaleY = scaleY;
        else
            throw new System.Exception("ObjectFlipper can ONLY be used to set scale values to -1, 1, or 0.");
    }

    /// <summary>
    /// Returns goal scaleY as an integer.
    /// </summary>
    public int GetGoalScaleY()
    {
        return _goalScaleY;
    }

    /// <summary>
    /// returns current scaleY value as a float.
    /// </summary>
    public float GetCurrentScaleY()
    {
        return transform.localScale.y;
    }

    /// <summary>
    /// Instantly snaps scale to goal scale without smoothing.
    /// </summary>
    public void SnapToGoal()
    {
        Vector3 snapScale = transform.localScale;
        snapScale.x = _goalScaleX;
        snapScale.y = _goalScaleY;
        transform.localScale = snapScale;
    }
}
