using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneRewinder : MonoBehaviour
{
    [SerializeField, Tooltip("Used to Pause/Rewind the animator.")]
    private Animator _anim;

    private bool _isReady = false; // indicates when rewind key will start working

    public void PlayCutscenePauseAudio()
    {
        // called by animator
        // play SFX
        AudioManager.Instance.PlayCutscenePause();
    }

    public void PauseCutscene()
    {
        // pause playback speed
        _anim.speed = 0;

        _isReady = true;
    }

    public void EndRewindSection()
    {
        _isReady = false;

        _anim.speed = 1;
    }

    #region Undo Controls
    private InputActionAsset _actions;

    private void OnEnable()
    {
        // Undo (press and release, to handle holding to undo)
        _actions = InputSystem.actions;
        _actions.actionMaps[0].FindAction("Undo").started += Undo;
        _actions.actionMaps[0].FindAction("Undo").canceled += Undo;
        _actions.actionMaps[0].Enable();
    }

    private void OnDisable()
    {
        _actions.actionMaps[0].FindAction("Undo").started += Undo;
        _actions.actionMaps[0].FindAction("Undo").canceled += Undo;
    }

    [Header("Undo")]
    [SerializeField, Tooltip("delay between first and second undo steps. Longer to prevent accidental double undo.")]
    private float _firstUndoDelay = 0.5f;
    [SerializeField, Tooltip("delay between release and animator reaching 0 speed.")]
    private float _releaseDelay;
    [SerializeField, Tooltip("delay between undo steps when undo key is being held.")]
    private float _undoDelay = 0.2f;
    [SerializeField, Tooltip("Speed of rewinding animation. 1 = same speed of initial animation.")]
    private float _rewindSpeed;

    private float _undoTimer = 0f;
    private bool _isUndoing = false;

    // Update is called once per frame
    void Update()
    {
        // Undoing player actions
        HandleHoldingUndo();
    }

    /// <summary>
    /// Handles undoing on one frame on button press, and cancelling the hold state on button release.
    /// </summary>
    private void Undo(InputAction.CallbackContext context)
    {
        // only process inputs AFTER reaching the end and pausing initially
        if (!_isReady)
            return;

        // start undo
        if (context.started)
        {
            // start holding state
            _isUndoing = true;

            // start/restart delay timer
            _undoTimer = _firstUndoDelay;

            // negative speed while undoing
            _anim.speed = _rewindSpeed;

            // undo SFX
            AudioManager.Instance.PlayRewind();
        }

        // release undo
        if (context.canceled)
        {
            _isUndoing = false;

            _undoTimer = _releaseDelay;
        }
    }

    /// <summary>
    /// Handles repeatedly undoing after delay intervals, assuming the undo key is still held.
    /// </summary>
    private void HandleHoldingUndo()
    {
        // only process inputs AFTER reaching the end and pausing initially
        if (!_isReady)
            return;

        // Undo is being held 
        if (_isUndoing)
        {
            // negative speed while undoing
            _anim.speed = _rewindSpeed;

            if (_undoTimer < 0) // ready to undo another frame
            {
                // start/restart delay timer
                _undoTimer = _undoDelay;

                // Undo SFX
                AudioManager.Instance.PlayRewind();
            }
            else
                _undoTimer -= Time.deltaTime;
        }
        else
        {
            // skip decrement logic if already at 0 speed
            if (_anim.speed == 0)
                return;

            // gradually fade OUT of undo state -> return to 0 speed
            float newSpeed = _anim.speed - (_rewindSpeed * Time.deltaTime / _releaseDelay);
            if (newSpeed <= 0)
                _anim.speed = 0;
            else
                _anim.speed = newSpeed;

            _undoTimer -= Time.deltaTime;
        }
    }
    #endregion
}
