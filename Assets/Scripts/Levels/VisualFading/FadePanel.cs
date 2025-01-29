using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles reading fade state from game manager and fading panel tilemap components.
/// </summary>
public class FadePanel : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField, Tooltip("Alpha level that the panel will fade to.")]
    private float _fadeAlpha;

    [Header("References")]
    [SerializeField, Tooltip("Used to fade the ground tilemap.")]
    private Tilemap _groundTilemap;
    [SerializeField, Tooltip("Used to fade the border tilemap.")]
    private Tilemap _borderTilemap;

    private bool _isFaded = false;

    // Start is called before the first frame update
    void Start()
    {
        UpdateFadeState();
    }

    // Update is called once per frame
    void Update()
    {
        // check for updating fade state
        if (_isFaded != GameManager.Instance.IsFading)
            UpdateFadeState();
    }

    /// <summary>
    /// Updates the display state of the panel's fading.
    /// </summary>
    private void UpdateFadeState()
    {
        // fetch new fading state
        _isFaded = GameManager.Instance.IsFading;

        Color newColor;
        // ground tilemap
        newColor = _groundTilemap.color;
        newColor.a = _isFaded ? _fadeAlpha : 1f;
        _groundTilemap.color = newColor;
        // border tilemap
        newColor = _borderTilemap.color;
        newColor.a = _isFaded ? _fadeAlpha : 1f;
        _borderTilemap.color = newColor;
    }
}
