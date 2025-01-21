using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelNode : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Level string name associated with the current node. \"None\" if not a level node.")]
    public string SceneName = "None";

    [Header("Adjacent Nodes")]
    public NodeConnectionData[] NodeConnections;

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
    public TravelNode GetConnection(NodeConnectionData.Direction dir)
    {
        foreach (NodeConnectionData node in NodeConnections)
        {
            if (node.Dir == dir)
            {
                if (node.Unlocked)
                    return node.Node;
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
