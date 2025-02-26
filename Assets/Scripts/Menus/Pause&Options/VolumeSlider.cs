using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using TMPro;

/// <summary>
/// Handles functionality related to slider and input field for volume configuration.
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    [Header("Components")]
    [SerializeField, Tooltip("Used for ensuring slider always matches input field.")]
    Slider _volumeSlider;
    [SerializeField, Tooltip("Used for ensuring input field always matches slider value.")]
    TMP_InputField _volumeInput;

    [Header("Configuration")]
    [SerializeField, Tooltip("Whether the slider is for master volume.")]
    private bool _isMasterVolume;
    [SerializeField, Tooltip("Whether the slider is for music volume.")]
    private bool _isMusicVolume;
    [SerializeField, Tooltip("Whether the slider is for sfx volume.")]
    private bool _isSfxVolume;

    // Start is called before the first frame update
    void Start()
    {
        // check for correct configuration
        int count = 0;
        if (_isMasterVolume) count++;
        if (_isMusicVolume) count++;
        if (_isSfxVolume) count++;

        if (count != 1)
            throw new System.Exception("Incorrect configuration of VolumeSlider. Must only be for ONE volume type");

        // set slider and input field to loaded value
        _volumeSlider.SetValueWithoutNotify(math.remap(0, 200, 0, 1, GetVolume()));
        _volumeInput.SetTextWithoutNotify(GetVolume().ToString());
    }

    /// <summary>
    /// handles updating of volume value based on slider change.
    /// Updates text accordingly and game manager.
    /// </summary>
    public void SliderUpdate()
    {
        // read input from slider
        int intVolume = Mathf.RoundToInt(_volumeSlider.value * 200);

        // display text as a percent (0% to 200%)
        _volumeInput.text = intVolume.ToString();

        UpdateVolume(intVolume);
    }

    /// <summary>
    /// handles updating of volume value based on input field change.
    /// Upadtes slider accordingly and game manager.
    /// </summary>
    public void InputFieldUpdate()
    {
        // read input from input field
        int intVolume = Mathf.RoundToInt(int.Parse(_volumeInput.text));
        // clamp input to valid range
        if (intVolume > 200)
        {
            intVolume = 200;
            _volumeInput.text = "200";
        }
        if (intVolume < 0) intVolume = 0;

        // remap from 0% to 200% volume to standard 0 to 1 range
        _volumeSlider.value = math.remap(0, 200, 0, 1, intVolume);

        UpdateVolume(intVolume);
    }

    /// <summary>
    /// Updates volume stored in game manager. 
    /// Accounts for volume type this volume slider is configured for.
    /// </summary>
    private void UpdateVolume(int newVolume)
    {
        if (_isMasterVolume)
            GameManager.Instance.MasterVolume = newVolume;
        if (_isMusicVolume)
            GameManager.Instance.MusicVolume = newVolume;
        if (_isSfxVolume)
            GameManager.Instance.SfxVolume = newVolume;
    }

    /// <summary>
    /// Gets volume stored in game manager. 
    /// Accounts for volume type this volume slider is configured for.
    /// </summary>
    private int GetVolume()
    {
        if (_isMasterVolume)
            return GameManager.Instance.MasterVolume;
        if (_isMusicVolume)
            return GameManager.Instance.MusicVolume;
        if (_isSfxVolume)
            return GameManager.Instance.SfxVolume;

        throw new System.Exception("Improper Usage of Volume Slider. Must be configured to a specific volume type.");
    }

    /// <summary>
    /// Reconfigures the slider and corresponding game manager value to default values for this slider.
    /// </summary>
    public void ResetDefault()
    {
        _volumeSlider.SetValueWithoutNotify(0.5f);
        _volumeInput.SetTextWithoutNotify("100");
        UpdateVolume(100);
    }
}
