using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unique functionality to the audio options sub-menu.
/// </summary>
public class AudioOptions : MonoBehaviour
{
    [SerializeField, Tooltip("Used to reset each slider to defaults")]
    private VolumeSlider[] _sliders;

    /// <summary>
    /// Sets all sliders and corresponding game manager sound values to defaults.
    /// </summary>
    public void RestoreDefaultAudio()
    {
        foreach (VolumeSlider slider in _sliders)
            slider.ResetDefault();
    }
}
