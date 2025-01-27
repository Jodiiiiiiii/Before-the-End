using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeConnectionData;

/// <summary>
/// Manages data and state pertaining to a level or non-level node for player level navigation.
/// </summary>
public class TravelNode : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Level string name associated with the current node. \"None\" if not a level node.")]
    public string SceneName = "None";

    [Header("Adjacent Nodes")]
    [Tooltip("Data for adjacent node connections of the current node.")]
    public NodeConnectionData[] NodeConnections;
    [Tooltip("Level number of current level; OR (for non-level nodes) list of all adjacent level numbers.")]
    public int[] LevelNums;

    [Header("Visuals")]
    [SerializeField, Tooltip("Game objects to enable when there is a blocked connection on an accessible node. Provided in up-right-down-left order.")]
    private GameObject[] _obstructionIndicators;
    [SerializeField, Tooltip("Used to swap the sprite to a completed variant.")]
    private SpriteRenderer _renderer;
    [SerializeField, Tooltip("Sprite variant for completed level.")]
    private Sprite _completedSprite;

    private void Awake()
    {
        // Configuration verification: must have associated level index
        if (LevelNums.Length < 1)
            throw new System.Exception("Each travel node MUST have at least one associated level.");

        // unlocking only needs to occur in start since the states will not change without the player leaving and re-entering the scene

        // determine locked/unlocked connections
        bool isUnlocked = false;
        foreach (int num in LevelNums)
        {
            if (GameManager.Instance.SaveData.LevelsComplete[num])
            {
                // update icon of node sprite (level nodes only)
                if (SceneName != "None")
                    _renderer.sprite = _completedSprite;

                isUnlocked = true;
                break;
            }
        }

        // unlock all connections (IN BOTH DIRECTIONS)
        if (isUnlocked)
        {
            foreach (NodeConnectionData connection in NodeConnections)
            {
                // unlock current connection
                connection.Unlocked = true;

                // unlock connection back as well
                TravelNode otherNode = connection.Node;
                foreach (NodeConnectionData otherConnection in otherNode.NodeConnections)
                {
                    if (otherConnection.Node == this)
                    {
                        otherConnection.Unlocked = true;
                        break;
                    }
                }
            }
        }
    }

    private void Start()
    {
        // skip processing for non-level nodes - they CANNOT have obstruction indicators
        if (SceneName == "None")
            return;

        // dynamically toggle blocked indicators
        // must be in start so it happens after ALL unlock state determinations

        // determine if we need to check for enabling barricades
        bool isAccessible = false;
        foreach (NodeConnectionData connection in NodeConnections)
        {
            if (connection.Unlocked)
            {
                isAccessible = true;
                break;
            }
        }
        // Special Case: first level is ALWAYS accessible
        if (!isAccessible)
        {
            foreach (int i in LevelNums)
            {
                if (i == 0)
                {
                    isAccessible = true;
                    break;
                }
            }
        }

        // check each direction for a locked connection since as least one is unlocked (i.e. node is accessible)
        if (isAccessible)
        {
            foreach (NodeConnectionData connection in NodeConnections)
            {
                if (!connection.Unlocked)
                {
                    EnableObstruction(connection.Dir);
                }
            }
        }
    }

    /// <summary>
    /// Enables the obstruction in the provided direction.
    /// </summary>
    private void EnableObstruction(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                _obstructionIndicators[0].SetActive(true);
                break;
            case Direction.Right:
                _obstructionIndicators[1].SetActive(true);
                break;
            case Direction.Down:
                _obstructionIndicators[2].SetActive(true);
                break;
            case Direction.Left:
                _obstructionIndicators[3].SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Rounds the nearest integer position of the current node.
    /// Note: the nodes should be integer aligned in Unity already.
    /// </summary>
    public Vector2Int GetTravelPos()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    /// <summary>
    /// Returns traversable node found in given direction.
    /// The node must exist AND be unlocked, otherwise null is returned.
    /// </summary>
    public NodeConnectionData GetConnection(NodeConnectionData.Direction dir)
    {
        foreach (NodeConnectionData node in NodeConnections)
        {
            if (node.Dir == dir)
            {
                if (node.Unlocked)
                    return node;
                else
                    return null; // found node is still locked
            }
        }

        // no connection in that direction found
        return null;
    }

    #region Popup
    [Header("Popup")]
    [SerializeField, Tooltip("Used to fade popup canvas in and out smoothly.")]
    private CanvasGroup _canvasFader;
    [SerializeField, Tooltip("Rate of change of alpha value.")]
    private float _alphaChangeRate;
    [SerializeField, Tooltip("Delay after arriving at new level when popup starts fading in.")]
    private float _fadeInDelay;

    private bool _fadeIn = false;

    /// <summary>
    /// Shows popup canvas for level description.
    /// Does nothing in the case of a travel node without a level.
    /// </summary>
    public void EnablePopup()
    {
        if (SceneName != "None")
            StartCoroutine(DoFadeInAfterDelay());
    }

    /// <summary>
    /// Hides popup canvas for level description.
    /// Does nothing in the case of a travel node without a level.
    /// </summary>
    public void DisablePopup()
    {
        if (SceneName != "None")
        {
            StopAllCoroutines(); // cancel fade in delay to prevent strange ordering.
            _fadeIn = false;
        }
    }

    private IEnumerator DoFadeInAfterDelay()
    {
        yield return new WaitForSeconds(_fadeInDelay);
        _fadeIn = true;
    }

    private void Update()
    {
        // skip popup processing for non-level nodes
        if (SceneName == "None")
            return;

        // smoothly fade in and out
        if (_fadeIn)
        {
            _canvasFader.alpha += _alphaChangeRate * Time.deltaTime;
            if (_canvasFader.alpha > 1) _canvasFader.alpha = 1;
        }
        else
        {
            _canvasFader.alpha -= _alphaChangeRate * Time.deltaTime;
            if (_canvasFader.alpha < 0) _canvasFader.alpha = 0;
        }
    }
    #endregion
}