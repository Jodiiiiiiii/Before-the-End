using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField, Tooltip("Distance from goal position when object will snap to exact goal position")] private float _snappingThreshold = 0.01f;
    [SerializeField, Tooltip("'Snappiness' of object seeking goal position")] private float _movingSharpness = 30f;

    private Vector2Int _goalPos;

    // Start is called before the first frame update
    void Start()
    {
        // ensure it starts at even integers
        _goalPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        // retrieve current position as Vector2
        Vector2 currPos = new Vector2(transform.position.x, transform.position.y);

        // Snap to index if close enough
        if (Vector2.Distance(currPos, _goalPos) < _snappingThreshold){
            Vector3 snapPos = transform.position;
            snapPos.x = _goalPos.x;
            snapPos.y = _goalPos.y;
            transform.position = snapPos;
        }
        else // smoothly lerp towards goal
        {
            Vector2 lerpVec2 = Vector2.Lerp(currPos, _goalPos, 1f - Mathf.Exp(-_movingSharpness * Time.deltaTime));
            Vector3 lerpPos = transform.position;
            lerpPos.x = lerpVec2.x;
            lerpPos.y = lerpVec2.y;
            transform.position = lerpPos;
        }
    }

    public void SetGoal(int x, int y)
    {
        _goalPos = new Vector2Int(x, y);
    }
}
