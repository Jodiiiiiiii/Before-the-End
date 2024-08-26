using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PanelDragging : MonoBehaviour
{
    // constants
    private const int MOUSE_LEFT = 0; // input constant

    [SerializeField] private Tilemap _borderTilemap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(MOUSE_LEFT))
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos); // convert to world space pos
            var tilePos = _borderTilemap.WorldToCell(mousePos);
            Debug.Log(tilePos.x + "," + tilePos.y);

            Debug.Log(_borderTilemap.origin);
        }
    }
}
