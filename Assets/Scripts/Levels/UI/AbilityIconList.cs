using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

public class AbilityIconList : MonoBehaviour
{
    [SerializeField, Tooltip("Icon prefab objects which are configured by this script.")]
    private AbilityIcon[] _icons;

    private PlayerControls _player;

    private int _currIndex = 0; // starts at 0 (first) by default

    // Start is called before the first frame update
    void Start()
    {
        // find player ONCE at the start - not ideal
        _player = GameObject.Find("Player").GetComponent<PlayerControls>();

        // first icon is the current one at start
        _icons[0].IsCurrent = true;

        // Initialize dinosaur animation types
        int i = 0;
        foreach(DinoType type in _player.GetDinoTypes())
        {
            _icons[i].SetAnimation(System.Enum.GetName(typeof(DinoType), type));

            i++;
        }

        // Disable remaining icons (no current dinosaur)
        for(; i < _icons.Length; i++)
        {
            _icons[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // swap current icon when change occurs
        if(_currIndex != _player.GetCurrDinoIndex())
        {
            _icons[_currIndex].IsCurrent = false;
            _currIndex = _player.GetCurrDinoIndex();
            _icons[_currIndex].IsCurrent = true;
        }
    }
}
