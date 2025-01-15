using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to fade out the upper portion of a tree when a level object or the player would be hidden behind it.
/// Keeps the better fully opaque visuals while not making it difficult to see objects when they are behind it.
/// </summary>
public class TreeFader : MonoBehaviour
{
    [SerializeField, Tooltip("Used to get position of current object.")]
    private Mover _mover;
    [SerializeField, Tooltip("Sprite to make faded when object/player is behind it.")]
    private SpriteRenderer _treeTopSprite;
    [SerializeField, Tooltip("Alpha level of faded tree top sprite.")]
    private float _fadeAlpha;

    private Mover _playerMover = null;

    private void OnEnable()
    {
        UndoHandler.ActionOccur += CheckAtEndOfFrame;
        UndoHandler.UndoOccur += CheckAtEndOfFrame;
    }

    private void OnDisable()
    {
        UndoHandler.ActionOccur -= CheckAtEndOfFrame;
        UndoHandler.UndoOccur -= CheckAtEndOfFrame;
    }

    private void CheckAtEndOfFrame()
    {
        StartCoroutine(WaitCheckAtEndOfFrame());
    }

    private IEnumerator WaitCheckAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        UpdateLogic();
    }

    /// <summary>
    /// Detects object or player and sets tree top transparency accordingly.
    /// </summary>
    private void UpdateLogic() // late update ensures checks are action-aligned
    {
        // position of tree top
        Vector2Int checkPos = _mover.GetGlobalGridPos() + Vector2Int.up;

        // skip object/player detection if tree top is not even visible currently
        if (!VisibilityChecks.IsVisible(_mover.gameObject, checkPos.x, checkPos.y))
        {
            Color tempCol = _treeTopSprite.color;
            tempCol.a = 1;
            _treeTopSprite.color = tempCol;

            return;
        }

        // check for object behind tree top
        QuantumState foundObj = VisibilityChecks.GetObjectAtPos(_mover, checkPos.x, checkPos.y);

        // attempt to find player reference if its in the same panel
        if (_playerMover is null)
            TryInitializePlayer();

        // set to faded if
        // (1) there is an obstructed object; but the object is NOT another tree
        // (2) the player is obstructed by the tree
        Color col = _treeTopSprite.color;
        if ((foundObj is not null && foundObj.ObjData.ObjType != ObjectType.Tree)
            || (_playerMover is not null && _playerMover.GetGlobalGridPos().Equals(checkPos)))
        {
            col.a = _fadeAlpha;
            _treeTopSprite.color = col;
        }
        // set to default (not faded)
        else
        {
            col.a = 1;
            _treeTopSprite.color = col;
        }   
    }

    /// <summary>
    /// Tries to find the player's mover component.
    /// Fails initialization and returns if player is not located in same panel as the current tree object.
    /// </summary>
    private void TryInitializePlayer()
    {
        // retrieve panel parent of tree
        if (_mover.transform.parent is not null && _mover.transform.parent.parent is not null
            && _mover.transform.parent.parent.TryGetComponent(out SortingOrderHandler panel))
        {
            // retrieve player object
            PlayerControls player = panel.transform.GetChild(1).GetComponentInChildren<PlayerControls>();
            if (player is null)
            {
                // player not in same panel - cannot be found
                return;
            }
            else
            {
                // retrieve player mover
                if (_playerMover is null && !player.TryGetComponent(out _playerMover))
                    throw new System.Exception("Player MUST have Mover component.");
            }
        }
        else
            throw new System.Exception("Tree Object MUST be a child of an 'Objects' object within a panel");
    }
}
