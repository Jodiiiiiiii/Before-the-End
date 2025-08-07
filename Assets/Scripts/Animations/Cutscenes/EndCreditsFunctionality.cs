using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// includes audio and scene transition functions to be used in the end credits scene.
/// </summary>
public class EndCreditsFunctionality : MonoBehaviour
{
    [SerializeField, Tooltip("Used to formally call scene transitions.")]
    private SceneTransitionHandler _transitionHandler;

    public void PlayCreditsMusic()
    {
        // queue track
        AudioManager.Instance.QueueCreditsMusic(false);
    }

    public void CutMusic()
    {
        // to prevent the track from looping weirdly
        AudioManager.Instance.UnqueueMusic();
    }

    public void TransitionOut()
    {
        // return to main menu
        _transitionHandler.LoadScene("MainMenu");
    }
}
