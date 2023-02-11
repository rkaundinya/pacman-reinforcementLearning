using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BitmapCode
{
    Wall,
    Pellet,
    PowerPellet,
    EatenGhost,
    FrightenedGhost,
    Ghost,
    Pacman,
}

public class StateRepresentation : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    private Tilemap[] tileMaps;
    // Bitmap representation of game to feed RL DNN
    private Hashtable dnnBitMap;

    // Start is called before the first frame update
    void Awake()
    {
        dnnBitMap = new Hashtable();

        tileMaps = grid.gameObject.GetComponentsInChildren<Tilemap>();

        BitmapCode tileCode = BitmapCode.Wall;
        foreach (Tilemap tileMap in tileMaps)
        {
            if (tileMap.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                tileCode = BitmapCode.Wall;   
            }
            else if (tileMap.gameObject.layer == LayerMask.NameToLayer("Pellet"))
            {
                tileCode = BitmapCode.Pellet;
            }

            foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
            {
                if (!tileMap.HasTile(position))
                {
                    continue;
                }

                dnnBitMap.Add(tileMap.GetCellCenterWorld(position), tileCode);
                if (tileCode == BitmapCode.Wall)
                {
                    Debug.DrawRay(tileMap.GetCellCenterWorld(position), Vector2.up, Color.red, 60);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
