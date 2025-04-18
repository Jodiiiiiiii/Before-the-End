using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles reading fade state from game manager and fading linked sprite renderer components.
/// Used on level objects that need to be faded.
/// </summary>
public class FadeLevelObject : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField, Tooltip("Alpha level that the panel will fade to.")]
    private float _fadeAlpha;

    [Header("References")]
    [SerializeField, Tooltip("Used to fade the sprites themselves tilemap.")]
    private SpriteRenderer[] _renderers;

    private bool _isFaded = false;
    private PanelStats _panel;

    private void Awake()
    {
        // Precondition: must be part of a panel
        // ensures panel does not start null
        if (transform.parent is null || transform.parent.parent is null || transform.parent.parent.parent is null
            || !transform.parent.parent.parent.TryGetComponent(out _panel))
            throw new System.Exception("Level Objects MUST be a child of Upper Objects OR Lower Objects, and their parent panel must have a PanelStats component.");
    }

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
        // ensure current stored panel is updated!
        // only calls TryGetComponent if the parent panel changes (this should happen infrequently only due to quantum entanglement swaps)
        if (transform.parent is null || transform.parent.parent is null || transform.parent.parent.parent is null)
            throw new System.Exception("Level Objects MUST always be a child of Upper Objects OR Lower Objects within a panel");
        if (_panel.transform != transform.parent.parent.parent)
        {
            if (!transform.parent.parent.parent.TryGetComponent(out _panel))
                throw new System.Exception("Level Object's containing parent MUST have a PanelStats component");
        }

        // fetch new fading state
        _isFaded = GameManager.Instance.IsFading;

        // update alpha of all linked sprite renderers
        Color newColor;
        foreach (SpriteRenderer renderer in _renderers)
        {
            newColor = renderer.color;
            newColor.a = _isFaded && !_panel.IsMainPanel() ? _fadeAlpha : 1f;
            renderer.color = newColor;
        }
    }
}
