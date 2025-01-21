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
        if (LevelName != "None")
            StartCoroutine(DoFadeInAfterDelay());
    }

    /// <summary>
    /// Hides popup canvas for level description.
    /// Does nothing in the case of a travel node without a level.
    /// </summary>
    public void DisablePopup()
    {
        if (LevelName != "None")
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
        if (LevelName == "None")
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
