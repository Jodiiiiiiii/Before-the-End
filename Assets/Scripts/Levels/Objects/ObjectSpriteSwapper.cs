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
    private ObjectType _currObjectType;
    private ObjectData _currObjectData;

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
        _currObjectType = _objState.ObjData.ObjType;
        _currObjectData = _objState.ObjData;
        UpdateSprites();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForChange();

        // Update quantum particles to match actual quantum state
        if (_quantumParticles.activeInHierarchy != _objState.IsQuantum())
            _quantumParticles.SetActive(_objState.IsQuantum());

        // update state for next check
        _currObjectData = _objState.ObjData;
    }

    /// <summary>
    /// Handles flipping, sprite swapping, and interfacing with the TwoFrameAnimator.
    /// </summary>
    public void CheckForChange()
    {
        bool visualChangeNeeded = false;
        // Calls to sprite flipper. update when there is a change - only swap on object change
        if (_currObjectType != _objState.ObjData.ObjType || _requiresFlip || _objState.ObjData.IsDisabled)
        {
            // EXIT: Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // sprite disable check if disabled object
                if (_objState.ObjData.IsDisabled)
                {
                    _animator.IsAnimated = false;
                    _renderer.sprite = null;
                }
                else // normal behavior
                {
                    // flip back to base scale
                    _flipper.SetScaleY((int)SPRITE_NORMAL);
                    // ensure sprite update occurs below
                    _currObjectType = _objState.ObjData.ObjType;
                    // ensure it no longer requires flip
                    _requiresFlip = false;
                    visualChangeNeeded = true;
                }
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _flipper.SetScaleY((int)SPRITE_SHRINK);
        }
        else // sprite should be shrinking if not yet at fully shrunk
            _flipper.SetScaleY((int)SPRITE_NORMAL);

        // only check for sprite updates if a change actually occurred
        // ALSO: there must be no change in any object data to skip update check (imported for example for water becoming a submerged log)
        if (!visualChangeNeeded && _currObjectData.Equals(_objState.ObjData))
            return;
        UpdateSprites();
    }

    private void UpdateSprites()
    {
        // set sprite properly based on object type and its type-specific data states
        switch (_currObjectType)
        {
            case ObjectType.Log:
                if (_objState.ObjData.IsOnFire) // fire variant (animated)
                {
                    _animator.IsAnimated = true;
                    _animator.UpdateSprites(_logSprites[1], _logSprites[2]);
                    _animator.UpdateVisuals();
                }
                else // normal logs (static)
                {
                    _animator.IsAnimated = false;
                    _renderer.sprite = _logSprites[0]; // normal variant
                }
                break;
            case ObjectType.Water:
                // check based on water state
                if (_objState.ObjData.WaterHasLog) // submerged log variant (animated)
                {
                    _animator.IsAnimated = true;
                    _animator.UpdateSprites(_waterSprites[2], _waterSprites[3]);
                    _animator.UpdateVisuals();
                }
                else if (_objState.ObjData.WaterHasRock) // submerged rock variant (animated)
                {
                    _animator.IsAnimated = true;
                    _animator.UpdateSprites(_waterSprites[4], _waterSprites[5]);
                    _animator.UpdateVisuals();
                }
                else // normal water (animated)
                {
                    _animator.IsAnimated = true;
                    _animator.UpdateSprites(_waterSprites[0], _waterSprites[1]);
                    _animator.UpdateVisuals();
                }
                break;
            case ObjectType.Rock:
                // normal rock - no animations
                _animator.IsAnimated = false;
                _renderer.sprite = _rockSprites[0];
                break;
            case ObjectType.Bush:
                if (_objState.ObjData.IsOnFire) // on fire variant (animated)
                {
                    _animator.IsAnimated = true;
                    _animator.UpdateSprites(_bushSprites[1], _bushSprites[2]);
                    _animator.UpdateVisuals();
                }
                else // normal bush (static)
                {
                    _animator.IsAnimated = false;
                    _renderer.sprite = _bushSprites[0];
                }
                break;
            case ObjectType.Tunnel:
                // normal tunnel (static)
                // set goal sprite to numbered tunnel based on tunnel index
                _animator.IsAnimated = false;
                _renderer.sprite = _tunnelSprites[_objState.ObjData.TunnelIndex];
                break;
            case ObjectType.Tree:
                // tree state will never change - and trees do not use this script
                break;
            case ObjectType.Clock:
                // normal clock (animated)
                _animator.IsAnimated = true;
                _animator.UpdateSprites(_clockSprites[0], _clockSprites[1]);
                _animator.UpdateVisuals();
                break;
            case ObjectType.Fire:
                // normal fire (animated)
                _animator.IsAnimated = true;
                _animator.UpdateSprites(_fireSprites[0], _fireSprites[1]);
                _animator.UpdateVisuals();
                break;
            case ObjectType.Void:
                // normal void (animated)
                _animator.IsAnimated = true;
                _animator.UpdateSprites(_voidSprites[0], _voidSprites[1]);
                _animator.UpdateVisuals();
                break;
            case ObjectType.Compy:
                // normal compy pair (animated)
                _animator.IsAnimated = true;
                _animator.UpdateSprites(_compySprites[0], _compySprites[1]);
                _animator.UpdateVisuals();
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