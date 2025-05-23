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
    private QuantumState _objState;
    [SerializeField, Tooltip("Used for calling actual calls to update player scale.")]
    private SpriteFlipper _flipper;
    [SerializeField, Tooltip("Used to actually update player sprite")]
    private SpriteRenderer _renderer;
    [SerializeField, Tooltip("Used to change sprites and toggle two frame animator.")]
    private TwoFrameAnimator _animator;

    [Header("Quantum")]
    [SerializeField, Tooltip("game object containing animated particle sprite")]
    private GameObject _quantumParticles;

    [Header("Sprites")]
    [SerializeField, Tooltip("sprites for log")]
    private Sprite[] _logSprites;
    [SerializeField, Tooltip("0 = water; 1 = water log; 2 = rock log")]
    private Sprite[] _waterSprites;
    [SerializeField, Tooltip("sprites for rock")]
    private Sprite[] _rockSprites;
    [SerializeField, Tooltip("sprites for bush")]
    private Sprite[] _bushSprites;
    [SerializeField, Tooltip("sprites for tunnel")]
    private Sprite[] _tunnelSprites;
    [SerializeField, Tooltip("sprites for clock")]
    private Sprite[] _clockSprites;
    [SerializeField, Tooltip("sprites for fire")]
    private Sprite[] _fireSprites;
    [SerializeField, Tooltip("sprites for void")]
    private Sprite[] _voidSprites;
    [SerializeField, Tooltip("sprites for compy")]
    private Sprite[] _compySprites;

    private bool _requiresFlip = false;
    private ObjectData _currObjectData;

    private Sprite _sprite1Queue = null;
    private Sprite _sprite2Queue = null;

    private bool _isActiveCoroutine = false;

    private void Awake()
    {
        // Precondition: all sprites are the correct lengths
        if (_logSprites.Length != 3)
            throw new System.Exception("There MUST be 3 log sprites (normal, burning two-frame).");
        if (_waterSprites.Length != 6)
            throw new System.Exception("There MUST be 6 water sprites (normal two-frame, log two-frame, rock two-frame).");
        if (_rockSprites.Length != 1)
            throw new System.Exception("There MUST be 1 rock sprite (normal).");
        if (_bushSprites.Length != 3)
            throw new System.Exception("There MUST be 3 bush sprites (normal, burning two-frame).");
        if (_tunnelSprites.Length != 6)
            throw new System.Exception("There MUST be 5 tunnel sprites (normal, numbered 1-5).");
        if (_clockSprites.Length != 2)
            throw new System.Exception("There MUST be 2 clock sprites (normal two-frame).");
        if (_fireSprites.Length != 2)
            throw new System.Exception("There MUST be 2 fire sprites (normal two-frame).");
        if (_voidSprites.Length != 2)
            throw new System.Exception("There MUST be 2 void sprites (normal two-frame).");
        if (_compySprites.Length != 2)
            throw new System.Exception("There MUST be 2 compy pair sprites (normal two-frame).");
    }

    private void Start()
    {
        _currObjectData = _objState.ObjData;

        // ensure correct initial configuration
        UpdateQueuedSprites();
        MatchObjectToQueue();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateQueuedSprites();

        CheckForChangeToFlip();

        // Update quantum particles to match actual quantum state
        if (_objState.ObjData.IsDisabled)
            _quantumParticles.SetActive(false); // do NOT show quantum particles for a destroyed object
        else if (_quantumParticles.activeInHierarchy != _objState.IsQuantum())
            _quantumParticles.SetActive(_objState.IsQuantum());

        // update state for next check
        _currObjectData = _objState.ObjData.CopyOf();
    }

    /// <summary>
    /// Handles flipping, sprite swapping, and interfacing with the TwoFrameAnimator.
    /// </summary>
    public void CheckForChangeToFlip()
    {
        // Calls to sprite flipper. Flips, then updates when:
        // (2) object data has changed! - the main one
        // (2) object just became disabled - stays shrunk (only half the flip) - part of object data changing
        // (3) manually flip call was made
        if (!_currObjectData.DataEqualsExceptTunnelRef(_objState.ObjData) || _requiresFlip)
        {
            // ensure only one flip is occuring at once
            StopCoroutine(FlipThenUpdate());
            StartCoroutine(FlipThenUpdate());
            
            // ensure call is only made once
            _requiresFlip = false;
        }
        // disabled objects become shrunk
        else if (!_isActiveCoroutine && _currObjectData.IsDisabled)
            _flipper.SetScaleY((int)SPRITE_SHRINK);
        // non-disabled objects become un-shrunk
        else if (!_isActiveCoroutine)
            _flipper.SetScaleY((int)SPRITE_NORMAL);
    }

    /// <summary>
    /// Conducts flip over time, shrinking, then sprite swapping, then growing back to main size.
    /// Disabled objects never exit this loop unless they are re-enabled.
    /// </summary>
    private IEnumerator FlipThenUpdate()
    {
        _isActiveCoroutine = true;

        while (true)
        {
            // EXIT: Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // to ensure no frame-perfect desync caused by state changes while at exact shrink scale of 0
                // THIS IS A POTENTIAL SOLUTION TO THE GHOST OBJECT MISALIGNMENT BUG
                UpdateQueuedSprites();

                MatchObjectToQueue();

                break; // end while loop, sprite has been shrinked and then swapped
            }
            else // sprite should be shrinking if not yet at fully shrunk
            {
                _flipper.SetScaleY((int)SPRITE_SHRINK);
            }

            yield return null;
        }

        _isActiveCoroutine = false;
    }

    private void UpdateQueuedSprites()
    {
        // disabled objects get no sprites
        if (_currObjectData.IsDisabled)
        {
            _sprite1Queue = null;
            _sprite2Queue = null;
        }

        // set sprite properly based on object type and its type-specific data states
        switch (_currObjectData.ObjType)
        {
            case ObjectType.Log:
                if (_objState.ObjData.IsOnFire) // fire variant (animated)
                {
                    _sprite1Queue = _logSprites[1];
                    _sprite2Queue = _logSprites[2];
                }
                else // normal logs (static)
                {
                    _sprite1Queue = _logSprites[0];
                    _sprite2Queue = null;
                }
                break;
            case ObjectType.Water:
                // check based on water state
                if (_objState.ObjData.WaterHasLog) // submerged log variant (animated)
                {
                    _sprite1Queue = _waterSprites[2];
                    _sprite2Queue = _waterSprites[3];
                }
                else if (_objState.ObjData.WaterHasRock) // submerged rock variant (animated)
                {
                    _sprite1Queue = _waterSprites[4];
                    _sprite2Queue = _waterSprites[5];
                }
                else // normal water (animated)
                {
                    _sprite1Queue = _waterSprites[0];
                    _sprite2Queue = _waterSprites[1];
                }
                break;
            case ObjectType.Rock:
                // normal rock - no animations
                _sprite1Queue = _rockSprites[0];
                _sprite2Queue = null;
                break;
            case ObjectType.Bush:
                if (_objState.ObjData.IsOnFire) // on fire variant (animated)
                {
                    _sprite1Queue = _bushSprites[1];
                    _sprite2Queue = _bushSprites[2];
                }
                else // normal bush (static)
                {
                    _sprite1Queue = _bushSprites[0];
                    _sprite2Queue = null;
                }
                break;
            case ObjectType.Tunnel:
                // normal tunnel (static)
                // set goal sprite to numbered tunnel based on tunnel index
                _sprite1Queue = _tunnelSprites[_objState.ObjData.TunnelIndex];
                _sprite2Queue = null;
                break;
            case ObjectType.Tree:
                // tree state will never change - and trees do not use this script
                break;
            case ObjectType.Clock:
                // normal clock (animated)
                _sprite1Queue = _clockSprites[0];
                _sprite2Queue = _clockSprites[1];
                break;
            case ObjectType.Fire:
                // normal fire (animated)
                _sprite1Queue = _fireSprites[0];
                _sprite2Queue = _fireSprites[1];
                break;
            case ObjectType.Void:
                // normal void (animated)
                _sprite1Queue = _voidSprites[0];
                _sprite2Queue = _voidSprites[1];
                break;
            case ObjectType.Compy:
                // normal compy pair (animated)
                _sprite1Queue = _compySprites[0];
                _sprite2Queue = _compySprites[1];
                break;
        }
    }

    /// <summary>
    /// Ensures sprites of the object match the queued sprites.
    /// Handles cases for static or 2-frame animated sprite states.
    /// </summary>
    private void MatchObjectToQueue()
    {
        // static sprite swap
        if (_sprite1Queue is not null && _sprite2Queue is null)
        {
            _animator.IsAnimated = false;
            _renderer.sprite = _sprite1Queue;
        }
        // animated sprite swap
        else if (_sprite1Queue is not null && _sprite2Queue is not null)
        {
            _animator.IsAnimated = true;
            _animator.UpdateSprites(_sprite1Queue, _sprite2Queue);
            _animator.UpdateVisuals();
        }
        // disabled state
        else if (_sprite1Queue is null && _sprite2Queue is null)
        {
            _animator.IsAnimated = false;
            _renderer.sprite = null;
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

    public void SkipFlip()
    {
        // prevent flip from occuring through standard logic
        _requiresFlip = false;
        _currObjectData = _objState.ObjData.CopyOf();

        // override update sprite
        UpdateQueuedSprites();
        MatchObjectToQueue();

        // ensure canceling of coroutines for smooth flipping
        StopCoroutine(FlipThenUpdate());
        _isActiveCoroutine = false;

        // ensure object is max scale
        _flipper.SetScaleY(1);
    }
}