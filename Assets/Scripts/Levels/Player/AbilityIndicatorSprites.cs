using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visibility of ability indicator sprites, accounting for visibility of the adjacent tile.
/// </summary>
public class AbilityIndicatorSprites : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _leftSprite;
    [SerializeField] private SpriteRenderer _rightSprite;
    [SerializeField] private SpriteRenderer _upSprite;
    [SerializeField] private SpriteRenderer _downSprite;

    [Header("Animation")]
    [SerializeField, Tooltip("Used to restart flickering animation when indicator sprites are enabled")]
    private Animator _anim;
    [SerializeField, Tooltip("Animation name of ability indicator flash effect")]
    private string _flashAnimName;

    [Header("Other Components")]
    [SerializeField, Tooltip("Needed to determine the position for visibility checks")]
    private Mover _objMover;

    private bool _isActive = false;
    private bool _prevActive = false;

    // Update is called once per frame
    void Update()
    {
        // handle initial visibility checks based on whether each grid pos is visible on the current panel
        if(_isActive && ! _prevActive)
        {
            // restart flashing indicators animation
            _anim.Play(_flashAnimName, -1, 0); // -1 indicates to not use animation layers, 0 means restart animation at start

            Vector2Int gridPos;

            gridPos = _objMover.GetGlobalGridPos() + Vector2Int.left;
            _leftSprite.enabled = VisibilityChecks.IsVisible(_objMover.gameObject, gridPos.x, gridPos.y);

            gridPos = _objMover.GetGlobalGridPos() + Vector2Int.right;
            _rightSprite.enabled = VisibilityChecks.IsVisible(_objMover.gameObject, gridPos.x, gridPos.y);

            gridPos = _objMover.GetGlobalGridPos() + Vector2Int.up;
            _upSprite.enabled = VisibilityChecks.IsVisible(_objMover.gameObject, gridPos.x, gridPos.y);

            gridPos = _objMover.GetGlobalGridPos() + Vector2Int.down;
            _downSprite.enabled = VisibilityChecks.IsVisible(_objMover.gameObject, gridPos.x, gridPos.y);
        }
        else if(!_isActive) // turn all off if not active
        {
            _leftSprite.enabled = false;
            _rightSprite.enabled = false;
            _upSprite.enabled = false;
            _downSprite.enabled = false;
        }

        _prevActive = _isActive;
    }

    /// <summary>
    /// enables ability indicator sprites
    /// </summary>
    public void SetAbilityActive(bool isActive)
    {
        _isActive = isActive;
    }
}
