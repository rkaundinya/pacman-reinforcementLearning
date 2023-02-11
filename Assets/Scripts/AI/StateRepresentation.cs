using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BitmapCode
{
    None,
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
            else
            {
                continue;
            }
        }
    }

    public void AddToBitmap(Vector3 location, BitmapCode type)
    {
        if (!dnnBitMap.Contains(location))
        {
            dnnBitMap.Add(location, type);
        }
    }

    public void UpdateStateValue(Vector3 location, BitmapCode newVal)
    {
        if (!dnnBitMap.Contains(location))
        {
            Debug.Log("Error - trying to update an invalid bitmap location");
            return;
        }

        // Only update bitmap value if incoming value is greater than current
        if ((int)newVal > (int)dnnBitMap[location])
        {
            Debug.Log(newVal.ToString());
            dnnBitMap[location] = newVal;
        }

        return;
    }

    public bool DebugCheckBitmapLoaction(Vector3 toCheck)
    {
        return dnnBitMap.Contains(toCheck);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
