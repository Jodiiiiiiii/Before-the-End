using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static NodeConnectionData;

/// <summary>
/// Handles all player controls in level select scene.
/// Controls include movement along preset level paths, entering a level, and pausing the game.
/// </summary>
public class LevelSelectControls : MonoBehaviour
{
    [Header("Initialization")]
    [SerializeField, Tooltip("Used to retrieve list of nodes and find the one of the correct index.")]
    private GameObject _levelWeb;

    private void Awake()
    {        
        // move player to current saved level node instantly
        TravelNode[] nodes = _levelWeb.GetComponentsInChildren<TravelNode>();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].LevelIdentifiers.Length == 1 && nodes[i].LevelIdentifiers[0] == GameManager.Instance.SaveData.CurrLevel)
            {
                ConfirmMove(ref nodes[i], true);
                break;
            }
        }
    }

    #region Controls Bindings
    private InputActionAsset _actions;

    private void OnEnable()
    {
        _actions = InputSystem.actions;

        // move presses (press only for level select)
        _actions.actionMaps[0].FindAction("Up").started += UpInput;
        _actions.actionMaps[0].FindAction("Down").started += DownInput;
        _actions.actionMaps[0].FindAction("Left").started += LeftInput;
        _actions.actionMaps[0].FindAction("Right").started += RightInput;
        // enter level
        _actions.actionMaps[0].FindAction("Ability").started += TryEnterLevel;
        // pause
        _actions.actionMaps[0].FindAction("Pause").started += PauseToggle;
        //_actions.actionMaps[0].FindAction("Help").started += PauseToggle;

        _actions.actionMaps[0].Enable();
    }

    private void OnDisable()
    {
        // remove move input bindings
        _actions.actionMaps[0].FindAction("Up").started -= UpInput;
        _actions.actionMaps[0].FindAction("Down").started -= DownInput;
        _actions.actionMaps[0].FindAction("Left").started -= LeftInput;
        _actions.actionMaps[0].FindAction("Right").started -= RightInput;
        // enter level
        _actions.actionMaps[0].FindAction("Ability").started -= TryEnterLevel;
        // pause
        _actions.actionMaps[0].FindAction("Pause").started -= PauseToggle;
        //_actions.actionMaps[0].FindAction("Help").started -= PauseToggle;

        _actions.actionMaps[0].Disable();
    }
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField, Tooltip("For actually activating movements.")]
    private Mover _playerMover;
    [SerializeField, Tooltip("Current TravelNode that the player is located at.")]
    private TravelNode _currNode;
    [SerializeField, Tooltip("Used to visually flip player in accordance to left/right movements.")]
    SpriteFlipper _flipper;

    /// <summary>
    /// Traverse player towards the upward connecting travel node, if it exists.
    /// </summary>
    private void UpInput(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart || GameManager.Instance.IsPaused)
            return;

        TryMove(Direction.Up);
    }

    /// <summary>
    /// Traverse player towards the right connecting travel node, if it exists.
    /// </summary>
    private void RightInput(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart || GameManager.Instance.IsPaused)
            return;

        _flipper.SetScaleX(-1); // face right (whether move occurs or not)

        TryMove(Direction.Right);
    }

    /// <summary>
    /// Traverse player towards the downward connecting travel node, if it exists.
    /// </summary>
    private void DownInput(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart || GameManager.Instance.IsPaused)
            return;

        TryMove(Direction.Down);
    }

    /// <summary>
    /// Traverse player towards the left connecting travel node, if it exists.
    /// </summary>
    private void LeftInput(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart || GameManager.Instance.IsPaused)
            return;

        _flipper.SetScaleX(1); // face left (whether move occurs or not)

        TryMove(Direction.Left);
    }

    /// <summary>
    /// Confirms movement and updating of current node of the player on successful move action.
    /// </summary>
    private void TryMove(Direction dir)
    {
        NodeConnectionData connection = _currNode.GetConnection(dir);
        
        // check for able to travel
        if (connection is not null && connection.Unlocked)
        {
            ConfirmMove(ref connection.Node);
        }
        else
        {
            // TODO: feedback for no node connection (player shake + negative feedback SFX?)
        }
    }

    /// <summary>
    /// Once logic has been done to determine a move can happen, this function will then do all the corresponding functionality
    /// </summary>
    private void ConfirmMove(ref TravelNode newNode, bool isInstant = false)
    {
        // move player to new node pos
        Vector2Int newPos = newNode.GetTravelPos();
        _playerMover.SetGlobalGoal(newPos.x, newPos.y);
        if (isInstant)
            _playerMover.SnapToGoal();

        // update popups
        _currNode.DisablePopup();
        newNode.EnablePopup();

        // update current node
        _currNode = newNode;

        // update currently selected level in game manager (used to track level completion) - only update on actual level nodes
        if (_currNode.SceneName != "None")
        {
            GameManager.Instance.SaveData.CurrLevel = _currNode.LevelIdentifiers[0];
        }
    }
    #endregion

    #region Enter Level
    [Header("Scene Transitions")]
    [SerializeField, Tooltip("Used to call animations of scene transitions.")]
    private SceneTransitionHandler _transitionHandler;

    private bool _isTransitionStart = false;

    /// <summary>
    /// Attempt to enter the level associated with the current node
    /// </summary>
    private void TryEnterLevel(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart || GameManager.Instance.IsPaused)
            return;

        // only able to enter level from actual level nodes
        if (_currNode.SceneName != "None")
        {
            // update help strings data for the current level
            string[] helpStrings = _currNode.GetHelpStrings();
            foreach (string helpStr in helpStrings)
            {
                if (!GameManager.Instance.SaveData.HelpUnlocks.Contains(helpStr))
                {
                    GameManager.Instance.SaveData.HelpUnlocks.Add(helpStr);
                    GameManager.Instance.SaveData.HelpNotif = true;
                }
            }

            _isTransitionStart = true;
            _transitionHandler.LoadScene(_currNode.SceneName);
        }
    }
    #endregion

    #region Pause
    /// <summary>
    /// Interfaces with the game manager to toggle the pause state of the game
    /// </summary>
    private void PauseToggle(InputAction.CallbackContext context)
    {
        // controls disabled during scene exit
        if (_isTransitionStart)
            return;

        // flip paused state
        GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
    }
    #endregion
}
