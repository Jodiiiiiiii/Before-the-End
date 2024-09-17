using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectData;

public class ObjectSpriteSwapper : MonoBehaviour
{
    private const float SPRITE_SHRINK = 0;
    private const float SPRITE_NORMAL = 1;

    [Header("Components")]
    [SerializeField, Tooltip("Used to access current object type")]
    private ObjectState _objState;
    [SerializeField, Tooltip("Used for calling actual calls to update player scale.")]
    private SpriteFlipper _flipper;
    [SerializeField, Tooltip("Used to actually update player sprite")]
    private SpriteRenderer _renderer;

    [Header("Sprites")]
    [SerializeField, Tooltip("sprites for log")]
    private Sprite[] _logSprites;
    [SerializeField, Tooltip("0 = water; 1 = water log; 2 = rock log")]
    private Sprite[] _waterSprites;
    [SerializeField, Tooltip("sprites for rock")]
    private Sprite[] _rockSprites;
    [SerializeField, Tooltip("sprites for tall rock")]
    private Sprite[] _tallRockSprites;
    [SerializeField, Tooltip("sprites for bush")]
    private Sprite[] _bushSprites;
    [SerializeField, Tooltip("sprites for tall bush")]
    private Sprite[] _tallBushSprites;
    [SerializeField, Tooltip("sprites for tunnel")]
    private Sprite[] _tunnelSprites;
    [SerializeField, Tooltip("sprites for item pickup")]
    private Sprite[] _pickupSprites;

    private Sprite _goalSprite;
    private bool _requiresFlip = false;
    private bool _isCoroutineActive = false;

    private void Start()
    {
        _goalSprite = _renderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        // actually update the goal sprite
        if (_objState.ObjData.IsDisabled) // disabled = no sprite
            _goalSprite = null;
        else
        {
            // set sprite properly based on object type and its type-specific data states
            switch (_objState.ObjData.ObjType)
            {
                case ObjectType.Log:
                    _goalSprite = _logSprites[0]; // no animations, just use 0
                    break;
                case ObjectType.Water:
                    // check based on water state
                    if (_objState.ObjData.WaterHasLog)
                        _goalSprite = _waterSprites[1];
                    else if (_objState.ObjData.WaterHasRock)
                        _goalSprite = _waterSprites[2];
                    else
                        _goalSprite = _waterSprites[0];
                    break;
                case ObjectType.Rock:
                    _goalSprite = _rockSprites[0]; // no animations, just use 0
                    break;
                case ObjectType.TallRock:
                    break;
                case ObjectType.Bush:
                    break;
                case ObjectType.TallBush:
                    break;
                case ObjectType.Tunnel:
                    break;
                case ObjectType.Pickup:
                    break;
            }
        }
        

        // call flipping coroutine ONLY if it is not already running
        // AND there is either a sprite change that needs to happen or it requires a flip
        if (!_isCoroutineActive && (_renderer.sprite != _goalSprite || _requiresFlip))
            StartCoroutine(FlipEffect());
    }

    /// <summary>
    /// Used to make sprites flip even when no sprite change occurs.
    /// Useful for quantum mechanic to indicate randomize even if no change occurred.
    /// </summary>
    public void RequireFlip()
    {
        _requiresFlip = true;
    }

    IEnumerator FlipEffect()
    {
        _isCoroutineActive = true;

        // Calls to sprite flipper. update when there is a change
        while (_renderer.sprite != _goalSprite || _requiresFlip)
        {
            // EXIT: Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // flip back to base scale
                _flipper.SetScaleY((int)SPRITE_NORMAL);

                // make sure flipping conditions are set to false
                _renderer.sprite = _goalSprite;
                _requiresFlip = false;
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _flipper.SetScaleY((int)SPRITE_SHRINK);    

            yield return null;
        }
        // ensures object NEVER gets stuck at scale 0 (invisible)
        _flipper.SetScaleY((int)SPRITE_NORMAL);

        _isCoroutineActive = false;
    }
}
