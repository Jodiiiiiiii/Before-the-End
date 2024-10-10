using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

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

    [Header("Sprites")]
    [SerializeField, Tooltip("sprites for stegosaurus")]
    private Sprite[] _stegoSprites;
    [SerializeField, Tooltip("sprites for triceratops")]
    private Sprite[] _trikeSprites;
    [SerializeField, Tooltip("sprites for ankylosaurus")]
    private Sprite[] _ankySprites;
    [SerializeField, Tooltip("sprites for spinosaurus")]
    private Sprite[] _spinoSprites;
    [SerializeField, Tooltip("sprites for pteranodon")]
    private Sprite[] _pteraSprites;
    [SerializeField, Tooltip("sprites for pyroraptor")]
    private Sprite[] _pyroSprites;
    [SerializeField, Tooltip("sprites for compsagnathus")]
    private Sprite[] _compySprites;

    private DinoType _spriteType;
    private bool _requiresFlip = false;

    private void Start()
    {
        _spriteType = _playerControls.GetCurrDinoType();
    }

    // Update is called once per frame
    void Update()
    {
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
            }
            else // sprite should be shrinking if not yet at fully shrunk
                _flipper.SetScaleY((int)SPRITE_SHRINK);
        }
        // ensures player NEVER gets stuck at scale 0 (invisible)
        else if (_flipper.GetCurrentScaleY() != SPRITE_NORMAL)
            _flipper.SetScaleY((int)SPRITE_NORMAL);
       
            
        // actually update the sprite when _spriteType is updated
        switch (_spriteType)
        {
            case DinoType.Stego:
                _renderer.sprite = _stegoSprites[0]; // currently not animated, just use 0
                break;
            case DinoType.Trike:
                _renderer.sprite = _trikeSprites[0]; // currently not animated, just use 0
                break;
            case DinoType.Anky:
                _renderer.sprite = _ankySprites[0]; // currently not animated, just use 0
                break;
            case DinoType.Pyro:
                break;
            case DinoType.Spino:
                break;
            case DinoType.Ptero:
                break;
            case DinoType.Compy:
                break;
        }
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
