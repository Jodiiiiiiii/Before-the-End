using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerControls;

public class AbilityIconList : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Icon prefab objects which are configured by this script.")]
    private AbilityIcon[] _icons;
    [SerializeField, Tooltip("Used to set animation of dino icon itself.")]
    private TwoFrameAnimator[] _animators;

    [Header("Sprites")]
    [SerializeField, Tooltip("Set of all dinosaur 2-frame animations. In order of 2-frames: stego, trike, anky, spino, pyro, ptera, compy.")]
    private Sprite[] _dinoFrames;

    private PlayerControls _player;

    private int _currIndex = 0; // starts at 0 (first) by default

    private void Awake()
    {
        // Precondition: 2 frames per dino
        if (_dinoFrames.Length != 14)
            throw new System.Exception("Each dino MUST have 2 associated animated frames.");
    }

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
            // update animators to properly corresponding frames
            _animators[i].UpdateSprites(_dinoFrames[(int)type * 2], _dinoFrames[(int)type * 2 + 1]);

            i++;
        }

        // Disable remaining icons (no current dinosaur)
        for(; i < _icons.Length; i++)
        {
            _icons[i].gameObject.SetActive(false);
        }

        // Initialize charges
        UpdateCharges();
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

        // Update in case of changes to remaining charges
        UpdateCharges();
    }

    private void UpdateCharges()
    {
        // Update Charges
        int[] charges = _player.GetAbilityCharges();
        for (int i = 0; i < charges.Length; i++)
        {
            _icons[i].SetCharges(charges[i]);
        }
    }
}
