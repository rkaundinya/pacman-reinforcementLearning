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
    public Vector3 gridTopLeft = new Vector3(-13.5f, -5.5f, 0f);
    public Vector3 gridBottomRight = new Vector3(-0.5f, -16.5f, 0f);
    public int mapGridRows = 12;
    public int mapGridCols = 14;

    [SerializeField]
    private Grid grid;
    private Tilemap[] tileMaps;
    // Bitmap representation of game to feed RL DNN
    private Hashtable dnnBitMap;
    //Array bitmap representation (ordered)
    private int[] orderedBitmap;
    private Vector3 lastPacmanLoc;

    // Start is called before the first frame update
    void Awake()
    {
        orderedBitmap = new int[mapGridRows * mapGridCols];
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

                    Vector3 centeredPos = tileMap.GetCellCenterWorld(position);
                    dnnBitMap.Add(centeredPos, (int)BitmapCode.Wall);
                    UpdateOrderedBitmap(centeredPos, BitmapCode.Wall);
                }
            }
            else
            {
                continue;
            }
        }

        // How to split flattened array back into row/column pairing
        /*int idx = 0;
        foreach (BitmapCode code in orderedBitmap)
        {
            if (code == BitmapCode.Wall)
            {
                int colIdx = idx % 14;
                int rowIdx = idx / 14;
                Debug.Log("Wall index is (" + rowIdx + "," + colIdx + ")");
            }

            idx++;
        }*/
    }

    private void UpdateOrderedBitmap(Vector3 location, BitmapCode type)
    {
        int colIdx = (int)(Mathf.Abs(location.x - gridTopLeft.x));
        int rowIdx = (int)(Mathf.Abs(location.y - gridTopLeft.y));

        // Debug.Log("Gridmap Position: (" + rowIdx + "," + colIdx + ")");

        int bitmapIdx = (rowIdx * mapGridCols) + colIdx;

        // Debug.Log("Bitmap Idx: " + bitmapIdx);

        orderedBitmap[bitmapIdx] = (int)type;
    }

    public BitmapCode GetCurrentBitmapLocationVal(Vector3 location)
    {
        int colIdx = (int)(Mathf.Abs(location.x - gridTopLeft.x));
        int rowIdx = (int)(Mathf.Abs(location.y - gridTopLeft.y));

        int bitmapIdx = (rowIdx * mapGridCols) + colIdx;

        return (BitmapCode)orderedBitmap[bitmapIdx];
    }

    public void AddToBitmap(Vector3 location, BitmapCode type)
    {
        if (!dnnBitMap.Contains(location))
        {
            dnnBitMap.Add(location, type);
            UpdateOrderedBitmap(location, type);
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
            UpdateOrderedBitmap(location, newVal);
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
            UpdateOrderedBitmap(location, newVal);
        }
    }

    public bool DebugCheckBitmapLoaction(Vector3 toCheck)
    {
        return dnnBitMap.Contains(toCheck);
    }

    public int[] GetBitcodeMap()
    {
        return orderedBitmap;
    }

    public void DebugPrintNumStatesInBitmap()
    {
        Debug.Log("NumOfBitmapStates" + dnnBitMap.Keys.Count);
    }
}
