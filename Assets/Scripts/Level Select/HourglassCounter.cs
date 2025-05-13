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
    [SerializeField, Tooltip("Color to indicate all levels complete.")]
    private Color _completionistColor;

    private void Awake()
    {
        // only track newer saved level identifiers (ignoring old format if they persist in save data)
        int count = 0;
        foreach (string str in GameManager.Instance.SaveData.LevelsComplete)
        {
            char[] arr = str.ToCharArray();
            if (arr.Length > 1 && (arr[0] == '1' || arr[0] == '2') && arr[1] == '-')
                count++;
        }

        _counterText.text = "" + count;

        // text color swap at 100% completion of game
        if (count >= 70)
            _counterText.color = _completionistColor;
    }
}
