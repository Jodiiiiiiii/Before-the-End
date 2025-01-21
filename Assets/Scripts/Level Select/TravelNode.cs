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

    /// <summary>
    /// Rounds the nearest integer position of the current node.
    /// Note: the nodes should be integer aligned in Unity already.
    /// </summary>
    public Vector2Int GetTravelPos()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
