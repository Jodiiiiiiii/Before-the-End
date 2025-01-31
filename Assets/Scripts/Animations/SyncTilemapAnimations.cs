using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Syncs the animations on a tilemap to match the AnimationManager frame rate.
/// Useful in the level select, where most elements are painted on tilemaps.
/// </summary>
public class SyncTilemapAnimations : MonoBehaviour
{
    [SerializeField, Tooltip("Used to set the naimation frame rate of all the linked tilemaps")]
    private Tilemap[] _tilemaps;

    private void Awake()
    {
        // inverse of time per frame is FPS
        foreach (Tilemap tilemap in _tilemaps)
        {
            //tilemap.animationFrameRate = 1f / AnimationManager.TIME_PER_FRAME;
        }   
    }
}
