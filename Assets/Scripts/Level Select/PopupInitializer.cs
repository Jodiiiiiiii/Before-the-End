using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static PlayerControls;

/// <summary>
/// Handles initial configuration of display name and dino list on level node popups.
/// </summary>
public class PopupInitializer : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField, Tooltip("Used to set text name of popup.")]
    private string _displayName;
    [SerializeField, Tooltip("Dinos to show in level popup.")]
    private DinoType[] _dinos;
    [SerializeField, Tooltip("Additional help unlock strings associated with the current level.")]
    private string[] _additionalHelpUnlocks;

    [Header("References")]
    [SerializeField, Tooltip("Used to set level name")]
    private TextMeshProUGUI _title;
    [SerializeField, Tooltip("Used to change spacing of layout group.")]
    private Beardy.GridLayoutGroup _layoutGroup;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _stego;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _trike;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _anky;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _spino;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _ptera;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _raptor;
    [SerializeField, Tooltip("Used to activate/deactivate stego.")]
    private GameObject _compy;

    // Start is called before the first frame update
    void Awake()
    {
        _title.text = _displayName;

        // center align when only one row
        if (_dinos.Length <= 3)
            _layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        // enable stego if nothing else in list
        if (_dinos.Length == 0)
            _stego.SetActive(true);

        // enable dino for each dino in list
        foreach (DinoType dino in _dinos)
        {
            switch(dino)
            {
                case DinoType.Stego:
                    _stego.SetActive(true);
                    break;
                case DinoType.Trike:
                    _trike.SetActive(true);
                    break;
                case DinoType.Anky:
                    _anky.SetActive(true);
                    break;
                case DinoType.Spino:
                    _spino.SetActive(true);
                    break;
                case DinoType.Ptera:
                    _ptera.SetActive(true);
                    break;
                case DinoType.Pyro:
                    _raptor.SetActive(true);
                    break;
                case DinoType.Compy:
                    _compy.SetActive(true);
                    break;
            }
        }
    }

    /// <summary>
    /// Returns list of help strings based on special strings for this level, and dino types of this level.
    /// </summary>
    public string[] GetHelpStrings()
    {
        string[] helpStrings = new string[_dinos.Length + _additionalHelpUnlocks.Length];

        for (int i = 0; i < _dinos.Length; i++)
            helpStrings[i] = _dinos[i].ToString();
        for (int i = 0; i < _additionalHelpUnlocks.Length; i++)
            helpStrings[i + _dinos.Length] = _additionalHelpUnlocks[i];

        return helpStrings;
    }
}
