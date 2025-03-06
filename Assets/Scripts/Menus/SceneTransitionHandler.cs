using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles smoothly animating and timing scene transitions.
/// TransitionIn plays automatically as default non-looping animation.
/// </summary>
public class SceneTransitionHandler : MonoBehaviour
{
    public static bool IsTransitioningOut = false;

    private void Start()
    {
        // always reset to false in new scene
        IsTransitioningOut = false;
    }

    [SerializeField, Tooltip("Used to trigger animations for scene enter/exit")]
    private Animator _anim;

    private bool _isDoneTransitioning = false;
    
    /// <summary>
    /// Function that should be used to activate any scene transition in the game.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        // do not permit any cases of loading a new scene when the LoadScene call has already been made elsewhere
        if (IsTransitioningOut)
            return;

        IsTransitioningOut = true;

        _anim.Play("TransitionOut");

        StartCoroutine(DoTransition(sceneName));
    }

    /// <summary>
    /// Handles waiting until animation completes before actually transitioning scenes
    /// </summary>
    private IEnumerator DoTransition(string sceneName)
    {
        yield return new WaitUntil(() => _isDoneTransitioning);

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Called by animation event at end of fade out animation
    /// </summary>
    public void SetDoneTransitioning()
    {
        _isDoneTransitioning = true;
    }
}
