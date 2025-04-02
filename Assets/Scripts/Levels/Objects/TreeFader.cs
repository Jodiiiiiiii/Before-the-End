using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Used to fade out the upper portion of a tree when a level object or the player would be hidden behind it.
/// Keeps the better fully opaque visuals while not making it difficult to see objects when they are behind it.
/// </summary>
public class TreeFader : MonoBehaviour
{
    [SerializeField, Tooltip("Used to get position of current object.")]
    private Mover _mover;
    [SerializeField, Tooltip("Base sprite used to compare alpha values to account for panel fading cross-cases.")]
    private SpriteRenderer _baseSprite;
    [SerializeField, Tooltip("Sprite to make faded when object/player is behind it.")]
    private SpriteRenderer _treeTopSprite;
    [SerializeField, Tooltip("Alpha level of faded tree top sprite.")]
    private float _fadeAlpha;

    private static Mover _playerMover = null;
    private bool _isTreeTopFading = false;
    private bool _isPanelFading = false;
    private InputActionAsset _actions;

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

        _actions = InputSystem.actions;
        _actions.actionMaps[0].FindAction("FadePanels").started += ControlCheck;
        _actions.actionMaps[0].FindAction("FadePanels").canceled += ControlCheck;
    }

    private void OnDisable()
    {
        UndoHandler.ActionOccur -= CheckAtEndOfFrame;
        UndoHandler.UndoOccur -= CheckAtEndOfFrame;

        _actions.actionMaps[0].FindAction("FadePanels").started -= ControlCheck;
        _actions.actionMaps[0].FindAction("FadePanels").canceled -= ControlCheck;
    }

    private void ControlCheck(InputAction.CallbackContext context)
    {
        CheckAtEndOfFrame();
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

        // Update tree top fade STATE ----------

        // skip object/player detection if tree top is not even visible currently
        if (!VisibilityChecks.IsVisible(_mover.gameObject, checkPos.x, checkPos.y))
        {
            // keep whatever fade/non-fade state it had - this ensures proper behavior with covering BOTH faded and non-faded trees

            // ONE EDGE CASE: if an object starts (1) BEHIND a tree and (2) ALSO BEHIND a panel
            // in this case, it takes one action frame once it has been revealed to have correct behavior for the rest of the level (including when undoing back to the start)
            // this case is acceptable as it is very niche and likely to be avoided regardless due to puzzle designing strategies

            return;
        }
        else
        {
            // check for object behind tree top
            QuantumState foundObj = VisibilityChecks.GetObjectAtPos(_mover, checkPos.x, checkPos.y);

            // set to faded if
            // (1) there is an obstructed object; but the object is NOT another tree
            // (2) the player is obstructed by the tree
            
            if ((foundObj is not null && foundObj.ObjData.ObjType != ObjectType.Tree)
                || _playerMover.GetGlobalGridPos().Equals(checkPos))
            {
                _isTreeTopFading = true;
            }
            // set to default (not faded)
            else
            {
                _isTreeTopFading = false;
            }
        }

        // Update ALPHA VALUE ----------------

        Color col = _treeTopSprite.color;
        // if object faded due to panel fading
        if (_baseSprite.color.a < 1f)
        {
            // match fade state of object itself
            col.a = _baseSprite.color.a;
            _treeTopSprite.color = col;
        }
        // otherwise fade entirely based on tree top fading logic above
        else
        {
            // something behind it
            if (_isTreeTopFading)
            {
                col.a = _fadeAlpha;
                _treeTopSprite.color = col;
            }
            // normal state
            else
            {
                col.a = 1;
                _treeTopSprite.color = col;
            }
        }
    }

    private void OnDestroy()
    {
        // ensures proper tree fading behavior even between multiple level scenes in a single session
        _playerMover = null;
    }
}
