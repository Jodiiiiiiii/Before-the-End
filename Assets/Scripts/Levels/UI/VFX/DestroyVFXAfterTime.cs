using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to clean up VFX after they are spawned and then completed.
/// </summary>
public class DestroyVFXAfterTime : MonoBehaviour
{
    [SerializeField, Tooltip("Time until this object is destroyed.")]
    private float _destroyTime;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyThis", _destroyTime);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
