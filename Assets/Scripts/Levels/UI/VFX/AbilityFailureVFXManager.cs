using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity that manages the placement of ability failure VFX through static function calls
/// </summary>
public class AbilityFailureVFXManager : MonoBehaviour
{
    private const int NULL_X = -10;
    private const int NULL_Y = 10;

    private static Vector2Int _queuedPos = new Vector2Int(NULL_X, NULL_Y);

    [Header("Ability Failure")]
    [SerializeField, Tooltip("Used to position the VFX image.")]
    private GameObject _vfxPositioner;
    [SerializeField, Tooltip("Used to play VFX flash animation.")]
    private Animator _anim;

    // Update is called once per frame
    void Update()
    {
        // only make a new VFX call if the queue position is NOT null
        if (!(_queuedPos.x == NULL_X && _queuedPos.y == NULL_Y))
        {
            // position at VFX position
            _vfxPositioner.transform.position = new Vector3(_queuedPos.x, _queuedPos.y, _vfxPositioner.transform.position.z);
            _anim.Play("AbilityFailureFlash", -1, 0f);

            // reset queued position
            _queuedPos = new Vector2Int(NULL_X, NULL_Y);

            // play ability fail SFX
            AudioManager.Instance.PlayAbilityFail();
        }
    }

    /// <summary>
    /// Plays ability failure VFX at the provided global grid coordinates.
    /// </summary>
    public static void PlayFailureVFX(Vector2Int globalPos)
    {
        _queuedPos = new Vector2Int(globalPos.x, globalPos.y);
    }
}
