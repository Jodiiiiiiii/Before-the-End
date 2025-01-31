using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

/// <summary>
/// Handles toggling the player between different sprite sets for each different dinosaur type, dynamically matching PlayerControls dino type.
/// </summary>
public class PlayerSpriteSwapper : MonoBehaviour
{
    private const float SPRITE_SHRINK = 0;
    private const float SPRITE_NORMAL = 1;

    [Header("Components")]
    [SerializeField, Tooltip("Used to access current dino type")]
    private PlayerControls _playerControls;
    [SerializeField, Tooltip("Used for calling actual calls to update player scale.")]
    private SpriteFlipper _flipper;
    [SerializeField, Tooltip("Used to actually update player sprite")]
    private SpriteRenderer _renderer;
    [SerializeField, Tooltip("Used to set sprites for two-frame animations.")]
    private TwoFrameAnimator _animator;

    [Header("Sprites")]
    [SerializeField, Tooltip("list of all STANDARD dino sprites; ordered in pairs in order: stego, trike, anky, spino, ptera, raptor, compy.")]
    private Sprite[] _dinoSprites;
    [SerializeField, Tooltip("Two sprites for spino swimming variant.")]
    private Sprite[] _spinoSwimSprites;
    [SerializeField, Tooltip("Two sprites for compy half variant.")]
    private Sprite[] _compyHalfSprites;

    private DinoType _spriteType;
    private bool _requiresFlip = false;

    private void Awake()
    {
        // Precondition: proper amount of sprites
        if (_dinoSprites.Length != 14 || _spinoSwimSprites.Length != 2 || _compyHalfSprites.Length != 2)
            throw new System.Exception("There must be 14 dino sprites, 2 spino swim sprites, AND 2 compy half sprites");
    }

    private void Start()
    {
        _spriteType = _playerControls.GetCurrDinoType();
    }

    // Update is called once per frame
    void Update()
    {
        bool visualChangeNeeded = false;
        // Calls to sprite flipper. update when there is a change
        if (_spriteType != _playerControls.GetCurrDinoType() || _requiresFlip)
        {
            // Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // flip back to base scale
                _flipper.SetScaleY((int)SPRITE_NORMAL);
                // ensure sprite update occurs
                _spriteType = _playerControls.GetCurrDinoType();
                // ensure it no longer requires flip
                _requiresFlip = false;
                visualChangeNeeded = true;
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _flipper.SetScaleY((int)SPRITE_SHRINK);
        }
        // ensures player NEVER gets stuck at scale 0 (invisible)
        else if (_flipper.GetCurrentScaleY() != SPRITE_NORMAL)
            _flipper.SetScaleY((int)SPRITE_NORMAL);

        // only check for sprite updates if a change actually occurred
        if (!visualChangeNeeded)
            return;
        // actually update the sprite when _spriteType is updated
        switch (_spriteType)
        {
            case DinoType.Stego:
                _animator.UpdateSprites(_dinoSprites[0], _dinoSprites[1]); // basic 2-frame animation
                break;
            case DinoType.Trike:
                _animator.UpdateSprites(_dinoSprites[2], _dinoSprites[3]); // basic 2-frame animation
                break;
            case DinoType.Anky:
                _animator.UpdateSprites(_dinoSprites[4], _dinoSprites[5]); // basic 2-frame animation
                break;
            case DinoType.Spino:
                // show EITHER grounded, or swimming variant
                if (!_playerControls.IsSwimming)
                    _animator.UpdateSprites(_dinoSprites[6], _dinoSprites[7]); // basic 2-frame animation
                else
                    _animator.UpdateSprites(_spinoSwimSprites[0], _spinoSwimSprites[1]); // spino swimming variant
                break;
            case DinoType.Ptera:
                _animator.UpdateSprites(_dinoSprites[8], _dinoSprites[9]); // basic 2-frame animation
                break;
            case DinoType.Pyro:
                _animator.UpdateSprites(_dinoSprites[10], _dinoSprites[11]); // basic 2-frame animation
                break;
            case DinoType.Compy:
                // show either full compy pack, or half compy pack variant
                if (!_playerControls.IsCompySplit())
                    _animator.UpdateSprites(_dinoSprites[12], _dinoSprites[13]); // basic 2-frame animation
                else
                    _animator.UpdateSprites(_compyHalfSprites[0], _compyHalfSprites[1]); // compy half variant
                break;
        }
        // ensure swap occurs instantly as necessary
        _animator.UpdateVisuals();
    }

    /// <summary>
    /// Used to make sprites flip even when no sprite change occurs.
    /// Useful for when player moved through tunnel (but dino type remains the same).
    /// </summary>
    public void RequireFlip()
    {
        _requiresFlip = true;
    }
}
