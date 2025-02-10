using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls when the camera moves to an adjacent region by tracking the position of the player in level select.
/// </summary>
public class CameraPanner : MonoBehaviour
{
    [Header("Zone Configuration")]
    [SerializeField, Tooltip("Leftmost grid position in first zone.")]
    private float _xStart;
    [SerializeField, Tooltip("Width of each zone.")]
    private int _width;
    [SerializeField, Tooltip("Bottommost grid position in first zone.")]
    private float _yStart;
    [SerializeField, Tooltip("Height of each zone.")]
    private int _height;

    [Header("References")]
    [SerializeField, Tooltip("Used to actually move the camera.")]
    private Mover _mover;
    [SerializeField, Tooltip("Used to access position of player.")]
    private GameObject _player;

    private int _currZoneX = 0; // start zone
    private int _currZoneY = 0; // start zone

    // Start is called before the first frame update
    void Start()
    {
        // player is set to position in Awake, so this works here
        // configure initial zone to match player start spot
        Vector2 startPos = _player.transform.position;
        _currZoneX = Mathf.RoundToInt(startPos.x / _width);
        _currZoneY = Mathf.RoundToInt(startPos.y / _height);

        // snap camera to player's starting zone at start
        UpdateCameraPos(true);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraPos();
    }

    /// <summary>
    /// Checks for player in a different region and pans the camera over accordingly.
    /// </summary>
    private void UpdateCameraPos(bool snapInstant = false)
    {
        bool changeOccurred = false;
        // check left bound
        float xBoundLeft = _xStart + _currZoneX * _width;
        if (_player.transform.position.x < xBoundLeft)
        {
            _currZoneX--;
            changeOccurred = true;
        }
        // check right bound
        float xBoundRight = _xStart + (_currZoneX + 1) * _width;
        if (_player.transform.position.x > xBoundRight)
        {
            _currZoneX++;
            changeOccurred = true;
        }
        // check down bound
        float yBoundDown = _yStart + _currZoneY * _height;
        if (_player.transform.position.y < yBoundDown)
        {
            _currZoneY--;
            changeOccurred = true;
        }
        // check up bound
        float yBoundUp = _yStart + (_currZoneY + 1) * _height;
        if (_player.transform.position.y > yBoundUp)
        {
            _currZoneY++;
            changeOccurred = true;
        }

        // prevents setting mover every frame
        if (changeOccurred || snapInstant)
            _mover.SetGlobalGoal(_width * _currZoneX, _height * _currZoneY);

        // snap if applicable
        if (snapInstant)
            _mover.SnapToGoal();
    }
}
