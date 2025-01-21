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
    public TravelNode _upNode;
    [Tooltip("Connecting node in right direction.")]
    public TravelNode _rightNode;
    [Tooltip("Connecting node in down direction.")]
    public TravelNode _downNode;
    [Tooltip("Connecting node in left direction.")]
    public TravelNode _leftNode;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
