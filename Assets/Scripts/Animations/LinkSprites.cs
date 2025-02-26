using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Links the sprites of two objects together.
/// Useful for outline effect.
/// Also handles color cycling on player outline.
/// </summary>
public class LinkSprites : MonoBehaviour
{
    [SerializeField, Tooltip("Sprite to be READ only and copied to the other.")]
    private SpriteRenderer _source;
    [SerializeField, Tooltip("Sprite to be assigned to the read sprite.")]
    private SpriteRenderer _target;

    [Header("Color Cycling")]
    [SerializeField, Tooltip("First color in cycle looper.")]
    private Color _color1;
    [SerializeField, Tooltip("First color in cycle looper.")]
    private Color _color2;

    private float _timer;
    private bool _forwardLerping;
    private int _prevFrameNum;

    private void Start()
    {
        // initial color data
        _timer = AnimationManager.TIME_PER_FRAME * 2;
        _prevFrameNum = AnimationManager.Instance.GetFrameNum();
        _forwardLerping = true;
        _target.color = _color1;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // ensure sprites are matching
        if (_source.sprite != _target.sprite)
            _target.sprite = _source.sprite;

        // lateUpdate is an attempt at minimizing single-frame desync issues


        // COLOR CYCLING
        _target.color = Color.Lerp(_forwardLerping ? _color2 : _color1, _forwardLerping ? _color1 : _color2, _timer / (AnimationManager.TIME_PER_FRAME * 2));
        
        // timer to update lerping direction
        _timer -= Time.deltaTime;
        
        // switch direction ever time animation manager returns to frame 0
        if (_prevFrameNum != AnimationManager.Instance.GetFrameNum() && _prevFrameNum == 1)
        {
            _timer = AnimationManager.TIME_PER_FRAME * 2;
            _forwardLerping = !_forwardLerping;
        }
        _prevFrameNum = AnimationManager.Instance.GetFrameNum();
    }
}
