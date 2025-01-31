using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates a sprite based on the two-frame global counter of the AnimationManager.
/// </summary>
public class TwoFrameAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("Sprites to swap between for the two-frame animation.")]
    private Sprite[] _sprites;
    [SerializeField, Tooltip("Renderer that will have its sprite toggled.")]
    private SpriteRenderer _renderer;

    private int _currFrame;

    private void Awake()
    {
        // Precondition: must have TWO sprites exactly
        if (_sprites.Length != 2)
            throw new System.Exception("TwoFrameAnimator MUST have two sprites assigned to it.");

        UpdateSprite();
    }

    // Update is called once per frame
    void Update()
    {
        // check for updating sprite
        if (_currFrame != AnimationManager.Instance.GetFrameNum())
            UpdateSprite();
    }

    /// <summary>
    /// Fetches current frame number and swaps sprite accordingly.
    /// </summary>
    private void UpdateSprite()
    {
        _currFrame = AnimationManager.Instance.GetFrameNum();
        _renderer.sprite = _sprites[_currFrame];
    }
}
