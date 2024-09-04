using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles static tracking of global and local stack frames.
/// determines when to accordingly call SaveFrame and UndoFrame functions on objects.
/// </summary>
public abstract class UndoHandler : MonoBehaviour
{
    protected static int _globalFrame = 0;

    public static void SaveFrame()
    {
        _globalFrame++;
    }

    public static void UndoFrame()
    {
        if(_globalFrame > 0)
            _globalFrame--;
    }

    [SerializeField, Tooltip("ObjectMover component; handles position changes")] 
    protected ObjectMover _objectMover; 
    
    protected int _localFrame = -1;

    // Update is called once per frame
    protected void Update()
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

    
}
