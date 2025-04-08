using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Allows for convenient static calls to trigger move VFX at target position
/// </summary>
public class MoveVFXManager : MonoBehaviour
{
    private const int NULL_X = -10;
    private const int NULL_Y = 10;

    private static Vector2Int _queuedPos = new Vector2Int(NULL_X, NULL_Y);
    private static Transform _spawnParent = null;
    private static int _spawnOrder = NULL_X;

    [Header("Move VFX")]
    [SerializeField, Tooltip("Prefab containing VFX for move action.")]
    private GameObject _moveVfxPrefab;

    // Update is called once per frame
    void Update()
    {
        // only make a new VFX call if the queue position is NOT null
        if (!(_queuedPos.x == NULL_X && _queuedPos.y == NULL_Y) && _spawnParent is not null && _spawnOrder != NULL_X)
        {
            // spawn effect at queued pos
            // spawn as a child of the same panel the player is in
            GameObject newVFX = Instantiate(_moveVfxPrefab, new Vector3(_queuedPos.x, _queuedPos.y, 0), _moveVfxPrefab.transform.rotation, _spawnParent);
            newVFX.GetComponent<Renderer>().sortingOrder = _spawnOrder;

            // reset queued position
            _queuedPos = new Vector2Int(NULL_X, NULL_Y);
            _spawnParent = null;
            _spawnOrder = NULL_X;
        }
    }

    /// <summary>
    /// Plays ability failure VFX at the provided global grid coordinates.
    /// </summary>
    public static void PlayMoveVfx(Vector2Int globalPos, PlayerControls player)
    {
        _queuedPos = new Vector2Int(globalPos.x, globalPos.y);

        // spawn parent is the panel itself, NOT the upper/lower objects empties (this could break some functionality)
        _spawnParent = player.transform.parent.parent;

        _spawnOrder = player.transform.parent.GetComponent<SortingGroup>().sortingOrder - 1;
    }
}
