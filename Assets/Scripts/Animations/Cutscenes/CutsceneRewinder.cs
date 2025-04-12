using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneRewinder : MonoBehaviour
{
    [SerializeField, Tooltip("Used to Pause/Rewind the animator.")]
    private Animator _anim;

    private bool _isReady = false; // indicates when rewind key will start working

    public void PlayCutscenePauseAudio()
    {
        // called by animator
        // play SFX
        AudioManager.Instance.PlayCutscenePause();
    }

    public void PauseCutscene()
    {
        // pause playback speed
        _anim.speed = 0;
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
