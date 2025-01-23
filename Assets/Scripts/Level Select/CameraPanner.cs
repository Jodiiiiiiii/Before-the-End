using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // snap camera to player's starting zone at start
        // player is set to position in Awake, so this works here

    }

    // Update is called once per frame
    void Update()
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
        if (changeOccurred)
            _mover.SetGlobalGoal(_width * _currZoneX, _height * _currZoneY);
    }
}
