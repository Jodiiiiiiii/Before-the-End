using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for convenient static calls to trigger move VFX at target position
/// </summary>
public class MoveVFXManager : MonoBehaviour
{
    private const int NULL_X = -10;
    private const int NULL_Y = 10;

    private static Vector2Int _queuedPos = new Vector2Int(NULL_X, NULL_Y);

    [Header("Move VFX")]
    [SerializeField, Tooltip("Prefab containing VFX for move action.")]
    private GameObject _moveVfxPrefab;

    // Update is called once per frame
    void Update()
    {
        // only make a new VFX call if the queue position is NOT null
        if (!(_queuedPos.x == NULL_X && _queuedPos.y == NULL_Y))
        {
            // spawn effect at queued pos
            Instantiate(_moveVfxPrefab, new Vector3(_queuedPos.x, _queuedPos.y, 0), _moveVfxPrefab.transform.rotation, transform);

            // reset queued position
            _queuedPos = new Vector2Int(NULL_X, NULL_Y);
        }
    }

    /// <summary>
    /// Plays ability failure VFX at the provided global grid coordinates.
    /// </summary>
    public static void PlayMoveVfx(Vector2Int globalPos)
    {
        _queuedPos = new Vector2Int(globalPos.x, globalPos.y);
    }
}
