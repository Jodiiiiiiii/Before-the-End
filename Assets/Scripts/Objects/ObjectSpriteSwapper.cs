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
    [SerializeField, Tooltip("sprites for water")]
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

    private ObjectType _spriteType;
    private bool _requiresFlip = false;

    private void Start()
    {
        _spriteType = _objState.ObjData.GetObjectType();
    }

    // Update is called once per frame
    void Update()
    {
        // Calls to sprite flipper. update when there is a change
        if (_spriteType != _objState.ObjData.GetObjectType() || _requiresFlip)
        {
            // Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // flip back to base scale
                _flipper.SetScaleY((int)SPRITE_NORMAL);
                // ensure sprite update occurs
                _spriteType = _objState.ObjData.GetObjectType();

                _requiresFlip = false;
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _flipper.SetScaleY((int)SPRITE_SHRINK);
        }
        // ensures object NEVER gets stuck at scale 0 (invisible)
        else if (_flipper.GetCurrentScaleY() != SPRITE_NORMAL)
            _flipper.SetScaleY((int)SPRITE_NORMAL);


        // actually update the sprite
        switch (_spriteType)
        {
            case ObjectType.Log:
                _renderer.sprite = _logSprites[0]; // no animations, just use 0
                break;
            case ObjectType.Water:
                _renderer.sprite = _waterSprites[0]; // currently no animations, just use 0
                break;
            case ObjectType.Rock:
                _renderer.sprite = _rockSprites[0]; // no animations, just use 0
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

    /// <summary>
    /// Used to make sprites flip even when no sprite change occurs.
    /// Useful for quantum mechanic to indicate randomize even if no change occurred.
    /// </summary>
    public void RequireFlip()
    {
        _requiresFlip = true;
    }
}
