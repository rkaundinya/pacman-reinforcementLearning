using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BitmapCode
{
    None,               //=0
    Wall,               //=1
    Pellet,             //=2
    PowerPellet,        //=3
    EatenGhost,         //=4
    FrightenedGhost,    //=5
    Ghost,              //=6
    Pacman,             //=7
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

        foreach (Tilemap tileMap in tileMaps)
        {
            if (tileMap.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
                {
                    if (!tileMap.HasTile(position))
                    {
                        continue;
                    }

                    dnnBitMap.Add(tileMap.GetCellCenterWorld(position), BitmapCode.Wall);
                }
            }
            else
            {
                continue;
            }
        }
    }

    public BitmapCode GetCurrentBitmapLocationVal(Vector3 location)
    {
        if (dnnBitMap.Contains(location))
        {
            return (BitmapCode)dnnBitMap[location];
        }

        return BitmapCode.None;
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
            dnnBitMap[location] = newVal;
        }

        return;
    }

    public void DemoteStateValue(Vector3 location, BitmapCode oldVal, BitmapCode newVal)
    {
        if (!dnnBitMap.Contains(location))
        {
            Debug.Log("Error - trying to update an invalid bitmap location");
            return;
        }

        if ((int)dnnBitMap[location] == (int)oldVal)
        {
            dnnBitMap[location] = newVal;
        }
    }

    public bool DebugCheckBitmapLoaction(Vector3 toCheck)
    {
        return dnnBitMap.Contains(toCheck);
    }

    public int[] GetBitcodeMap()
    {
        int[] toReturn = new int[dnnBitMap.Keys.Count];

        int idx = 0;
        foreach (var val in dnnBitMap.Values)
        {
            toReturn[idx] = (int)val;
            idx++;
        }

        return toReturn;
    }

    public void DebugPrintNumStatesInBitmap()
    {
        Debug.Log("NumOfBitmapStates" + dnnBitMap.Keys.Count);
    }
}
