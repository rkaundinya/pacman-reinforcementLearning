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
    private BitmapCode[] orderedBitmap;

    // Start is called before the first frame update
    void Awake()
    {
        orderedBitmap = new BitmapCode[mapGridRows * mapGridCols];



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
                    if (position == new Vector3(-14, -6, 0))
                    {
                        Debug.Log(centeredPos);
                    }

                    // How to convert position to flatttened array index
                    if (centeredPos == new Vector3(-13.5f, -5.5f, 0) || centeredPos == new Vector3(-0.5f, -16.5f, 0f))
                    {
                        int colIdx = (int)(Mathf.Abs(centeredPos.x - gridTopLeft.x));
                        int rowIdx = (int)(Mathf.Abs(centeredPos.y - gridTopLeft.y));

                        Debug.Log("Gridmap Position: (" + rowIdx + "," + colIdx + ")");

                        int bitmapIdx = (rowIdx * mapGridCols) + colIdx;
                        Debug.Log("Bitmap Idx: " + bitmapIdx);
                        orderedBitmap[bitmapIdx] = BitmapCode.Wall;
                    }

                    dnnBitMap.Add(tileMap.GetCellCenterWorld(position), BitmapCode.Wall);
                }
            }
            else
            {
                continue;
            }
        }

        // How to split flattened array back into row/column pairing
        int idx = 0;
        foreach (BitmapCode code in orderedBitmap)
        {
            if (code == BitmapCode.Wall)
            {
                int colIdx = idx % 14;
                int rowIdx = idx / 14;
                Debug.Log("Wall index is (" + rowIdx + "," + colIdx + ")");
            }

            idx++;
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
