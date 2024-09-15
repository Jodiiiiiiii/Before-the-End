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

    private DinoType _spriteType;

    private void Start()
    {
        _spriteType = _playerControls.GetCurrDinoType();
    }

    // Update is called once per frame
    void Update()
    {
        // Calls to sprite flipper. update when there is a change
        if (_spriteType != _playerControls.GetCurrDinoType())
        {
            // Ready to restore sprite to normal
            if (_flipper.GetCurrentScaleY() == SPRITE_SHRINK)
            {
                // flip back to base scale
                _flipper.SetScaleY((int)SPRITE_NORMAL);
                // ensure sprite update occurs
                _spriteType = _playerControls.GetCurrDinoType();
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
                break;
            case DinoType.Dilo:
                break;
            case DinoType.Bary:
                break;
            case DinoType.Ptero:
                break;
            case DinoType.Compy:
                break;
            case DinoType.Pachy:
                break;
        }
    }
}