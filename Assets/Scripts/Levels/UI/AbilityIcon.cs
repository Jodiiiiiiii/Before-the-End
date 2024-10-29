using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class AbilityIcon : MonoBehaviour
{
    [HideInInspector]
    public bool IsCurrent = false;

    [Header("Components")]
    [SerializeField, Tooltip("Used for updating border sprite.")]
    private Image _borderImage;
    [SerializeField, Tooltip("Used for updating indicator sprite.")]
    private Animator _dinoAnimator;

    [Header("Sprites")]
    [SerializeField, Tooltip("0 = default; 1 = current")]
    private Sprite[] _borderSprites;

    // Update is called once per frame
    void Update()
    {
        // Update border sprite
        if (IsCurrent)
            _borderImage.sprite = _borderSprites[1];
        else
            _borderImage.sprite = _borderSprites[0];
    }

    /// <summary>
    /// Sets animation to the provided dinosaur name
    /// </summary>
    public void SetAnimation(string name)
    {
        _dinoAnimator.Play(name);
    }
}
