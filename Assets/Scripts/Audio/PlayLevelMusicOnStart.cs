using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayLevelMusicOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 4 Biome Groupings (Forest, Mounatin, Swamp, Fire)
        // 2 variants of each track based on timeline
        string name = SceneManager.GetActiveScene().name;
        if (name.Contains("Forest") || name.Contains("Tutorial"))
            AudioManager.Instance.QueueForestMusic(name[0] == '2');
        else if (name.Contains("Mountain") || name.Contains("Lake") || name.Contains("Valley"))
            AudioManager.Instance.QueueMountainMusic(name[0] == '2');
        else if (name.Contains("Swamp") || name.Contains("Beach"))
            AudioManager.Instance.QueueSwampMusic(name[0] == '2');
        else if (name.Contains("Asteroid") || name.Contains("Wasteland"))
            AudioManager.Instance.QueueFireMusic(name[0] == '2');

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
