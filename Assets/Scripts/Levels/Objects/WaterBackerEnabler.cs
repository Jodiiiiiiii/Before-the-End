using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to enable/disable sprite renderer to match whether the object is water.
/// </summary>
public class WaterBackerEnabler : MonoBehaviour
{
    [SerializeField, Tooltip("Retrieves current object type.")]
    private QuantumState _objState;
    [SerializeField, Tooltip("Used to disable/enable sprite renderer.")]
    private SpriteRenderer _renderer;

    private bool _isWaterBacker;

    private void Start()
    {
        _isWaterBacker = _objState.ObjData.ObjType == ObjectType.Water;
        _renderer.enabled = _isWaterBacker;
    }

    // Update is called once per frame
    void Update()
    {
        // don't set enabled state every frame, so use a local bool buffer
        if (!_isWaterBacker && _objState.ObjData.ObjType == ObjectType.Water /*&& (_objState.ObjData.WaterHasLog || _objState.ObjData.WaterHasRock)*/)
        {
            _renderer.enabled = true;
            _isWaterBacker = true;
        }
        else if (_isWaterBacker && (_objState.ObjData.ObjType != ObjectType.Water /*|| (!_objState.ObjData.WaterHasLog && !_objState.ObjData.WaterHasRock)*/))
        {
            _renderer.enabled = false;
            _isWaterBacker = false;
        }
    }
}
