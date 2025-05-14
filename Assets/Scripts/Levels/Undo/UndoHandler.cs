using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles static tracking of global and local stack frames.
/// determines when to accordingly call SaveFrame and UndoFrame functions on objects.
/// </summary>
public abstract class UndoHandler : MonoBehaviour
{
    public delegate void ActionOccurred();
    public static event ActionOccurred ActionOccur;
    public delegate void UndoOccurred();
    public static event UndoOccurred UndoOccur;

    protected static int _globalFrame = 0;

    public static void SaveFrame()
    {
        // ensure all fire bushes are ticked before action frame ends
        FireSpreadHandler.UpdateFireTick();

        _globalFrame++;

        ActionOccur?.Invoke();
    }

    public static void UndoFrame()
    {
        if (_globalFrame > 0)
        {
            _globalFrame--;

            // undo SFX - only play on actual undo not just on attempt
            AudioManager.Instance.PlayRewind();

            UndoOccur?.Invoke();
        }
    }

    [SerializeField, Tooltip("Mover component; handles position changes")] 
    protected Mover _mover; 
    
    protected int _localFrame = -1;

    private void Start()
    {
        // ensure global frame resets to zero in each level (only on load)
        if(Time.timeSinceLevelLoad == 0)
            _globalFrame = 0;
    }

    // Update is called once per frame
    protected void LateUpdate()
    {
        // Progress game state (store new stack frame)
        if(_localFrame < _globalFrame)
        {
            _localFrame++;
            SaveStackFrame();
        }

        // undo game state (load previous stack frame)
        if (_localFrame > _globalFrame)
        {
            _localFrame--;
            UndoStackFrame();
        }
    }

    /// <summary>
    /// saves current frame of data relevant to panel object type.
    /// Should be called AFTER the change has fully taken place.
    /// </summary>
    protected abstract void SaveStackFrame();

    /// <summary>
    /// loades previous frame of data based on panel object's parameters
    /// </summary>
    protected abstract void UndoStackFrame();

    public static int GetGlobalFrame()
    {
        return _globalFrame;
    }
}
