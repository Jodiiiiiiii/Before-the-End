using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Links the sprites of two objects together.
/// Useful for outline effect
/// </summary>
public class LinkSprites : MonoBehaviour
{
    [SerializeField, Tooltip("Sprite to be READ only and copied to the other.")]
    private SpriteRenderer _source;
    [SerializeField, Tooltip("Sprite to be assigned to the read sprite.")]
    private SpriteRenderer _target;

    // Update is called once per frame
    void LateUpdate()
    {
        // ensure sprites are matching
        if (_source.sprite != _target.sprite)
            _target.sprite = _source.sprite;

        // lateUpdate is an attempt at minimizing single-frame desync issues
    }
}
