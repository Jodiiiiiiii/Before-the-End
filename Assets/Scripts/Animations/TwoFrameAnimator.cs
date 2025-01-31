using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates a sprite based on the two-frame global counter of the AnimationManager.
/// Handles animated swapping for both Image and SpriteRenderer components.
/// </summary>
public class TwoFrameAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("Whether the current sprite is being animated")]
    public bool IsAnimated = true;
    [SerializeField, Tooltip("Sprites to swap between for the two-frame animation.")]
    private Sprite[] _sprites;

    [Header("Reference - USE ONE ONLY")]
    [SerializeField, Tooltip("Renderer that will have its sprite toggled.")]
    private SpriteRenderer _renderer;
    [SerializeField, Tooltip("Image that will have its sprite toggled.")]
    private Image _img;

    private int _currFrame;

    private void Awake()
    {
        // Precondition: must have TWO sprites exactly
        if (_sprites.Length != 2)
            throw new System.Exception("TwoFrameAnimator MUST have two sprites assigned to it.");

        // Precondition: use Image OR SpriteRenderer
        if (((!_renderer ? 0 : 1) + (!_img ? 0 : 1)) != 1)
            throw new System.Exception("TwoFrameAnimator MUST have EITHER a linked Image or SpriteRenderer component. Not neither or both.");

        if (IsAnimated)
            UpdateVisuals();
    }

    // Update is called once per frame
    void Update()
    {
        // check for updating sprite
        if (IsAnimated && _currFrame != AnimationManager.Instance.GetFrameNum())
            UpdateVisuals();
    }

    /// <summary>
    /// Fetches current frame number and swaps sprite accordingly.
    /// </summary>
    public void UpdateVisuals()
    {
        _currFrame = AnimationManager.Instance.GetFrameNum();
        if (!_renderer)
            _img.sprite = _sprites[_currFrame];
        else
            _renderer.sprite = _sprites[_currFrame];
    }

    /// <summary>
    /// Changes the sprites being swapped between for the animation.
    /// Useful for dynamic configuration and for objects which can change during runtime,
    /// </summary>
    public void UpdateSprites(Sprite _frame0, Sprite _frame1)
    {
        _sprites[0] = _frame0;
        _sprites[1] = _frame1;
    }
}
