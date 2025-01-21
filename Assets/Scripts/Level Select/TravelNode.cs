using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelNode : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Level string name associated with the current node. \"None\" if not a level node.")]
    public string LevelName = "None";

    [Header("Adjacent Nodes")]
    [Tooltip("Connecting node in up direction.")]
    public TravelNode UpNode = null;
    [Tooltip("Connecting node in right direction.")]
    public TravelNode RightNode = null;
    [Tooltip("Connecting node in down direction.")]
    public TravelNode DownNode = null;
    [Tooltip("Connecting node in left direction.")]
    public TravelNode LeftNode = null;

    [Header("Other Components")]
    [SerializeField, Tooltip("Used to enable level popup when on level.")]
    private Canvas _popupCanvas;

    /// <summary>
    /// Rounds the nearest integer position of the current node.
    /// Note: the nodes should be integer aligned in Unity already.
    /// </summary>
    public Vector2Int GetTravelPos()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    /// <summary>
    /// Shows popup canvas for level description.
    /// Does nothing in the case of a travel node without a level.
    /// </summary>
    public void EnablePopup()
    {
        if (LevelName != "None")
            _popupCanvas.enabled = true;
    }

    /// <summary>
    /// Hides popup canvas for level description.
    /// Does nothing in the case of a travel node without a level.
    /// </summary>
    public void DisablePopup()
    {
        if (LevelName != "None")
            _popupCanvas.enabled = false;
    }
}
