using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [Tooltip("Level name of current level; OR (for non-level nodes) list of all adjacent level names.")]
    public string[] LevelIdentifiers;

    [Header("Visuals")]
    [SerializeField, Tooltip("Game objects to enable when there is a blocked connection on an accessible node. Provided in up-right-down-left order.")]
    private GameObject[] _obstructionIndicators;
    [SerializeField, Tooltip("Used to disable animation for completed node.")]
    private TwoFrameAnimator _anim;
    [SerializeField, Tooltip("Used to swap the sprite to a completed variant.")]
    private SpriteRenderer _renderer;
    [SerializeField, Tooltip("Sprite variant for completed level.")]
    private Sprite _completedSprite;

    [Header("Help Configuration")]
    [SerializeField, Tooltip("Used to access data for help strings.")]
    private PopupInitializer _popupInit;

    private void Awake()
    {
        // SPECIAL NODE: timeline traversal - ignores configuration (handled by other nodes)
        if (SceneName == "LevelSelect")
        {
            // disable timeline traversal node if timeline 2 was never unlocked yet
            if (SceneManager.GetActiveScene().name == "LevelSelect1" && !GameManager.Instance.SaveData.isSecondTimelineUnlock)
            {
                // disable connectiong node/connection
                NodeConnectionData adjNode = NodeConnections[0];
                foreach (NodeConnectionData otherConnection in adjNode.Node.NodeConnections)
                {
                    // sever connection to this node altogeher
                    // setting to null still works based on how GetConnection(dir) function is set up
                    if (otherConnection.Node == this)
                        otherConnection.Node = null;
                }

                // disable timeline traversal node
                gameObject.SetActive(false);
            }

            return;
        }

        // Configuration verification: must have associated level index
        if (LevelIdentifiers.Length < 1)
            throw new System.Exception("Each travel node MUST have at least one associated level.");

        // Fetch level select timelineNum
        string timelineNum;
        if (SceneManager.GetActiveScene().name == "LevelSelect1")
            timelineNum = "1-";
        else if (SceneManager.GetActiveScene().name == "LevelSelect2")
            timelineNum = "2-";
        else
            throw new System.Exception("Can Only use TravelNode.cs in level select scenes");

        // unlocking only needs to occur in start since the states will not change without the player leaving and re-entering the scene

        // determine locked/unlocked connections
        bool isUnlocked = false;
        foreach (string name in LevelIdentifiers)
        {
            if (GameManager.Instance.SaveData.LevelsComplete.Contains(timelineNum + name))
            {
                // update icon of node sprite (level nodes only)
                if (SceneName != "None")
                {
                    _anim.IsAnimated = false;
                    _renderer.sprite = _completedSprite;
                }

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
                if (otherNode is null) // if encountering the disabled special node, skip and process the next node in the list
                    continue;
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
        // timeline navigation node also cannot have obstruction indicator
        if (SceneName == "None" || SceneName == "LevelSelect")
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
            foreach (string i in LevelIdentifiers)
            {
                if (i.Equals("Tut0"))
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
                if (node.Node is not null && node.Unlocked)
                    return node;
                else
                    return null; // found node is still locked
            }
        }

        // no connection in that direction found
        return null;
    }

    #region Help Strings
    /// <summary>
    /// Returns list of help strings based on special strings for this level, and dino types of this level. 
    /// </summary>
    public string[] GetHelpStrings()
    {
        return _popupInit.GetHelpStrings();
    }
    #endregion

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

    #region CHEATS
    private void OnEnable()
    {
        if (LevelIdentifiers.Length == 1)
            CheatsControls.UnlockLevels += CheatUnlockThisLevel;
    }

    private void OnDisable()
    {
        if (LevelIdentifiers.Length == 1)
            CheatsControls.UnlockLevels -= CheatUnlockThisLevel;
    }

    /// <summary>
    /// Only add level if it has not already been completed.
    /// </summary>
    private void CheatUnlockThisLevel()
    {
        // Fetch level select timelineNum
        string timelineNum;
        if (SceneManager.GetActiveScene().name == "LevelSelect1")
            timelineNum = "1-";
        else if (SceneManager.GetActiveScene().name == "LevelSelect2")
            timelineNum = "2-";
        else
            throw new System.Exception("Can Only use TravelNode.cs in level select scenes");

        if (!GameManager.Instance.SaveData.LevelsComplete.Contains(timelineNum + LevelIdentifiers[0]) && LevelIdentifiers[0] != "Timeline")
            GameManager.Instance.SaveData.LevelsComplete.Add(timelineNum + LevelIdentifiers[0]);

        /*string[] helpStrings = GetHelpStrings();
        foreach (string helpStr in helpStrings)
        {
            if (!GameManager.Instance.SaveData.HelpUnlocks.Contains(helpStr))
            {
                GameManager.Instance.SaveData.HelpUnlocks.Add(helpStr);
                GameManager.Instance.SaveData.HelpNotif = true;
            }
        }*/
    }
    #endregion
}