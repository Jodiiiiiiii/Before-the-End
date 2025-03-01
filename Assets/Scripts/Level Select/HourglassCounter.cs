using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Configures text of hourglass counter to match game manager value each time level select is reloaded
/// </summary>
public class HourglassCounter : MonoBehaviour
{
    [SerializeField, Tooltip("Used to update text value on awake.")]
    private TextMeshProUGUI _counterText;

    private void Awake()
    {
        _counterText.text = "" + GameManager.Instance.SaveData.LevelsComplete.Count;
    }
}
