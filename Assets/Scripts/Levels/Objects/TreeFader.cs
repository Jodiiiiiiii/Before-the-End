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

    private static Mover _playerMover = null;

    private void Awake()
    {
        // only initialize once per ALL tree faders in the scene
        if (_playerMover is null)
        {
            // Initialize player mover
            GameObject player = GameObject.Find("Player");
            if (player is null)
                throw new System.Exception("There MUST be an object named \"Player\" in every level scene");
            if (!player.TryGetComponent(out _playerMover))
                throw new System.Exception("There MUST be only one object named \"Player\" in the level scene, and it must have a Mover component");
        }
    }

    private void Start()
    {
        // ensure proper state at start of scene
        CheckAtEndOfFrame();
    }

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

        // set to faded if
        // (1) there is an obstructed object; but the object is NOT another tree
        // (2) the player is obstructed by the tree
        Color col = _treeTopSprite.color;
        if ((foundObj is not null && foundObj.ObjData.ObjType != ObjectType.Tree)
            || _playerMover.GetGlobalGridPos().Equals(checkPos))
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
}
