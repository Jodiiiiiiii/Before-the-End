using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectControls : MonoBehaviour
{
    private void Start()
    {
        // configure starting node whenever entering scene based on save data of current node/level - rather than using scene as default
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
        // pause
        _actions.actionMaps[0].FindAction("Pause").started += PauseToggle;

        _actions.actionMaps[0].Enable();
    }

    private void OnDisable()
    {
        // remove move input bindings
        _actions.actionMaps[0].FindAction("Up").started -= UpInput;
        _actions.actionMaps[0].FindAction("Down").started -= DownInput;
        _actions.actionMaps[0].FindAction("Left").started -= LeftInput;
        _actions.actionMaps[0].FindAction("Right").started -= RightInput;

        // pause
        _actions.actionMaps[0].FindAction("Pause").started -= PauseToggle;

        _actions.actionMaps[0].Disable();
    }
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField, Tooltip("For actually activating movements.")]
    private Mover _playerMover;
    [SerializeField, Tooltip("Current TravelNode that the player is located at.")]
    private TravelNode _currNode;

    /// <summary>
    /// Traverse player towards the upward connecting travel node, if it exists.
    /// </summary>
    private void UpInput(InputAction.CallbackContext context)
    {
        if (_currNode.UpNode is not null)
        {
            ConfirmMove(ref _currNode.UpNode);
        }
        else
        {
            // TODO: feedback for no node connection (player shake + negative feedback SFX?)
        }
    }

    /// <summary>
    /// Traverse player towards the right connecting travel node, if it exists.
    /// </summary>
    private void RightInput(InputAction.CallbackContext context)
    {
        if (_currNode.RightNode is not null)
        {
            ConfirmMove(ref _currNode.RightNode);
        }
        else
        {
            // TODO: feedback for no node connection (player shake + negative feedback SFX?)
        }
    }

    /// <summary>
    /// Traverse player towards the downward connecting travel node, if it exists.
    /// </summary>
    private void DownInput(InputAction.CallbackContext context)
    {
        if (_currNode.DownNode is not null)
        {
            ConfirmMove(ref _currNode.DownNode);
        }
        else
        {
            // TODO: feedback for no node connection (player shake + negative feedback SFX?)
        }
    }

    /// <summary>
    /// Traverse player towards the left connecting travel node, if it exists.
    /// </summary>
    private void LeftInput(InputAction.CallbackContext context)
    {
        if (_currNode.LeftNode is not null)
        {
            ConfirmMove(ref _currNode.LeftNode);
        }
        else
        {
            // TODO: feedback for no node connection (player shake + negative feedback SFX?)
        }
    }

    /// <summary>
    /// Confirms movement and updating of current node of the player on successful move action.
    /// </summary>
    private void ConfirmMove(ref TravelNode newNode)
    {
        // move player to new node pos
        Vector2Int newPos = newNode.GetTravelPos();
        _playerMover.SetGlobalGoal(newPos.x, newPos.y);

        // update current node
        _currNode = newNode;
    }
    #endregion

    #region PAUSE
    /// <summary>
    /// Interfaces with the game manager to toggle the pause state of the game
    /// </summary>
    private void PauseToggle(InputAction.CallbackContext context)
    {
        // flip paused state
        GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
    }
    #endregion
}
