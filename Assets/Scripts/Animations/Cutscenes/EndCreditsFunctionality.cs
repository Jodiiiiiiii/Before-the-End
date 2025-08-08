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
        // used to determine if completionist credits music should play instead
        int count = 0;
        foreach (string str in GameManager.Instance.SaveData.LevelsComplete)
        {
            char[] arr = str.ToCharArray();
            if (arr.Length > 1 && (arr[0] == '1' || arr[0] == '2') && arr[1] == '-')
                count++;
        }

        // queue track
        AudioManager.Instance.QueueCreditsMusic(count >= 70);
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
