using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayLevelMusicOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // two variants - 1 = timeline 1 track, 2 = timeline 2 track
        if (SceneManager.GetActiveScene().name.ToCharArray()[0] == '1')
            AudioManager.Instance.QueueLevelMusic1();
        else
            AudioManager.Instance.QueueLevelMusic2();
    }
}
