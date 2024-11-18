using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityIcon : MonoBehaviour
{
    [HideInInspector]
    public bool IsCurrent = false;

    [Header("Components")]
    [SerializeField, Tooltip("Used for updating border sprite.")]
    private Image _borderImage;
    [SerializeField, Tooltip("Used for updating indicator sprite.")]
    private Animator _dinoAnimator;
    [SerializeField, Tooltip("Used for updating charges counter.")]
    private TextMeshProUGUI _chargesText;
    [SerializeField, Tooltip("Used for disabling charges counter when you have infinite charges.")]
    private GameObject _chargesObject;

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

    /// <summary>
    /// Updates text to show provided number.
    /// Hides charge indicator if -1 (infinite charges).
    /// </summary>
    public void SetCharges(int num)
    {
        if (num == -1)
        {
            _chargesObject.SetActive(false);
        }
        else
        {
            _chargesObject.SetActive(true);
            _chargesText.text = "" + num;
        }
    }
}
