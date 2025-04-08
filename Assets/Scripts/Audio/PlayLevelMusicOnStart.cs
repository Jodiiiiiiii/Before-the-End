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

        // determine whether or not to play ambient level fire track
        char[] sceneName = SceneManager.GetActiveScene().name.ToCharArray();
        // Ensure sceneName is long enough to prevent nullRef
        // EITHER (1) Was (wasteland) level OR (2) Ast (asteroid) level
        if (sceneName.Length >= 5 && (
            (sceneName[2] == 'W' && sceneName[3] == 'a' && sceneName[4] == 's') 
            || (sceneName[2] == 'A' && sceneName[3] == 's' && sceneName[4] == 't')))
        {
            AudioManager.Instance.EnableAmbientLevelFire();
        }
    }
}
