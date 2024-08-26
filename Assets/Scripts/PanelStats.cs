using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelStats : MonoBehaviour
{
    [SerializeField, Tooltip("Whether this panel is the immovable main panel")] private bool _isMainPanel = false;

    [SerializeField, Tooltip("# of tiles of width of the panel")] public float Width;
    [SerializeField, Tooltip("# of tiles of height of the panel (not including 1 unit dragging bar")] public float Height;

    public float OriginX { get; private set; }
    public float OriginY { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        // ensure updated on first frame
        OriginX = transform.position.x;
        OriginY = transform.position.y - Height; // shift stored origin to bottom left
    }

    // Update is called once per frame
    void Update()
    {
        OriginX = transform.position.x;
        OriginY = transform.position.y - Height; // shift stored origin to bottom left
    }
}
