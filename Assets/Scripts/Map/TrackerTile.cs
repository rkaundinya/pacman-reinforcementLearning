using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerTile : MonoBehaviour
{
    private Hashtable activeBitmapCodes;
    private bool pacmanStartingTile = false;

    private void Awake()
    {
        activeBitmapCodes = new Hashtable();
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterWithBitmap();

        if (gameObject.transform.position == GameManager.gm.pacman.startingPos)
        {
            pacmanStartingTile = true;
            Invoke(nameof(RemoveStartingTilePacmanState), 0.2f);

            GameManager.gm.pacmanStateReset = () =>
            {
                Invoke(nameof(RemoveStartingTilePacmanState), 0.2f);
            };
        }
    }

    private void RegisterWithBitmap()
    {
        activeBitmapCodes.Add(BitmapCode.None, 0);

        GameManager.gm.stateRepresentation.AddToBitmap(gameObject.transform.position, BitmapCode.None);
    }

    // Hacky way to make sure the starting tile deregisters pacman's starting pos after game start
    private void RemoveStartingTilePacmanState()
    {
        BitmapCode currentMaxBitmapCode = GetMaxCurrentStateBitmapCode();
        GameManager.gm.stateRepresentation.DemoteStateValue(gameObject.transform.position, BitmapCode.Pacman, currentMaxBitmapCode);

        Debug.Log("Updated starting tile val: " + (int)GameManager.gm.stateRepresentation.GetCurrentBitmapLocationVal(gameObject.transform.position));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector3 currentLocation = gameObject.transform.position;

        // Update bitmap with ghost code
        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            GhostFrightened ghostFrightened = other.gameObject.GetComponent<GhostFrightened>();
            // Check if ghost is frightened or not and update accordingly
            if (ghostFrightened.enabled)
            {
                if (ghostFrightened.eaten)
                {
                    if (activeBitmapCodes.Contains(BitmapCode.EatenGhost))
                    {
                        activeBitmapCodes[BitmapCode.EatenGhost] = (int)activeBitmapCodes[BitmapCode.EatenGhost] + 1;
                    }
                    else
                    {
                        activeBitmapCodes[BitmapCode.EatenGhost] = 1;
                    }

                    GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, BitmapCode.EatenGhost);
                    return;
                }
                else
                {

                    if (activeBitmapCodes.Contains(BitmapCode.FrightenedGhost))
                    {
                        activeBitmapCodes[BitmapCode.FrightenedGhost] = (int)activeBitmapCodes[BitmapCode.FrightenedGhost] + 1;
                    }
                    else
                    {
                        activeBitmapCodes[BitmapCode.FrightenedGhost] = 1;
                    }

                    GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, BitmapCode.FrightenedGhost);
                    return;
                }
            }
            else
            {
                if (activeBitmapCodes.Contains(BitmapCode.Ghost))
                {
                    activeBitmapCodes[BitmapCode.Ghost] = (int)activeBitmapCodes[BitmapCode.Ghost] + 1;
                }
                else
                {
                    activeBitmapCodes[BitmapCode.Ghost] = 1;
                }

                GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, BitmapCode.Ghost);
                return;
            }
        }
        // Update bitmap with pacman code and consume pellet
        else if (other.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            activeBitmapCodes.Add(BitmapCode.Pacman, 1);

            GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, BitmapCode.Pacman);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Vector3 currentLocation = gameObject.transform.position;
        BitmapCode removingCode = BitmapCode.None;

        // Update bitmap with ghost code
        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            GhostFrightened ghostFrightened = other.gameObject.GetComponent<GhostFrightened>();
            // Check if ghost is frightened or not and update accordingly
            if (ghostFrightened.enabled)
            {
                // If ghost is eaten
                if (ghostFrightened.eaten)
                {
                    removingCode = BitmapCode.EatenGhost;
                    if (activeBitmapCodes.Contains(removingCode))
                    {
                        // Used to keep track of how many duplicate bitmap states tile is in
                        int bitmapStateValCnt = (int)activeBitmapCodes[removingCode];

                        // Either decrement count or remove state entirely from tile's state tracking
                        if (bitmapStateValCnt > 1)
                        {
                            activeBitmapCodes[removingCode] = bitmapStateValCnt - 1;
                        }
                        else
                        {
                            activeBitmapCodes.Remove(removingCode);
                        }
                    }
                    else
                    {
                        Debug.Log("Error - trying to remove non-existing tile state");
                    }
                }
                // Ghost is frightened but not eaten
                else
                {
                    removingCode = BitmapCode.FrightenedGhost;
                    if (activeBitmapCodes.Contains(removingCode))
                    {
                        // Used to keep track of how many duplicate bitmap states tile is in
                        int bitmapStateValCnt = (int)activeBitmapCodes[removingCode];

                        // Either decrement count or remove state entirely from tile's state tracking
                        if (bitmapStateValCnt > 1)
                        {
                            activeBitmapCodes[removingCode] = bitmapStateValCnt - 1;
                        }
                        else
                        {
                            activeBitmapCodes.Remove(removingCode);
                        }
                    }
                    else
                    {
                        Debug.Log("Error - trying to remove non-existing tile state");
                    }
                }
            }
            else
            {
                removingCode = BitmapCode.Ghost;
                if (activeBitmapCodes.Contains(removingCode))
                {
                    // Used to keep track of how many duplicate bitmap states tile is in
                    int bitmapStateValCnt = (int)activeBitmapCodes[removingCode];

                    // Either decrement count or remove state entirely from tile's state tracking
                    if (bitmapStateValCnt > 1)
                    {
                        activeBitmapCodes[removingCode] = bitmapStateValCnt - 1;
                    }
                    else
                    {
                        activeBitmapCodes.Remove(removingCode);
                    }
                }
                else
                {
                    Debug.Log("Error - trying to remove non-existing tile state");
                }
            }
        }
        // Update bitmap with pacman code
        else if (other.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            removingCode = BitmapCode.Pacman;
            if (activeBitmapCodes.Contains(removingCode))
            {
                activeBitmapCodes.Remove(removingCode);
            }
            else
            {
                Debug.Log("Error - trying to remove non-existing tile state");
            }
        }

        BitmapCode currentMaxBitmapCode = GetMaxCurrentStateBitmapCode();
        GameManager.gm.stateRepresentation.DemoteStateValue(currentLocation, removingCode, currentMaxBitmapCode);
    }

    protected virtual BitmapCode GetMaxCurrentStateBitmapCode()
    {
        int highestKey = -1;
        BitmapCode codeToReturn = BitmapCode.None;

        foreach (var Key in activeBitmapCodes.Keys)
        {
            if ((int)Key > highestKey)
            {
                highestKey = (int)Key;
                codeToReturn = (BitmapCode)Key;
            }
        }

        return codeToReturn;
    }

    public void Reset()
    {
        activeBitmapCodes.Clear();
        RegisterWithBitmap();
    }
}
