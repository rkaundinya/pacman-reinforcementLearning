using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Pellet : MonoBehaviour
{
    public int points = 10;
    private bool eaten = false;
    protected Hashtable activeBitmapCodes;

    public void Awake()
    {
        activeBitmapCodes = new Hashtable();
    }

    public void Start()
    {
        RegisterWithBitmap();
    }

    protected virtual void RegisterWithBitmap()
    {
        activeBitmapCodes.Add(BitmapCode.Pellet, 0);
        GameManager.gm.stateRepresentation.AddToBitmap(gameObject.transform.position, BitmapCode.Pellet);
    }

    protected virtual void Eat()
    {
        activeBitmapCodes.Remove(BitmapCode.Pellet);
        GameManager.gm.PelletEaten(this);
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
            // Consume pellet
            Eat();
            eaten = true;

            activeBitmapCodes.Add(BitmapCode.Pacman, 1);
            Debug.Log("Added pacman to pellet bitmap code");

            GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, BitmapCode.Pacman);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Vector3 currentLocation = gameObject.transform.position;
        BitmapCode currentMaxBitmapCode = GetMaxCurrentStateBitmapCode();

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
                    if (activeBitmapCodes.Contains(BitmapCode.EatenGhost))
                    {
                        // Used to keep track of how many duplicate bitmap states tile is in
                        int bitmapStateValCnt = (int)activeBitmapCodes[BitmapCode.EatenGhost];

                        // Either decrement count or remove state entirely from pellet's state tracking
                        if (bitmapStateValCnt > 1)
                        {
                            activeBitmapCodes[BitmapCode.EatenGhost] = bitmapStateValCnt - 1;
                        }
                        else
                        {
                            activeBitmapCodes.Remove(BitmapCode.EatenGhost);
                        }
                    }
                    else
                    {
                        Debug.Log("Error - trying to remove non-existing pellet state");
                    }

                    GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, currentMaxBitmapCode);
                    return;
                }
                // Ghost is frightened but not eaten
                else
                {
                    if (activeBitmapCodes.Contains(BitmapCode.FrightenedGhost))
                    {
                        // Used to keep track of how many duplicate bitmap states tile is in
                        int bitmapStateValCnt = (int)activeBitmapCodes[BitmapCode.FrightenedGhost];

                        // Either decrement count or remove state entirely from pellet's state tracking
                        if (bitmapStateValCnt > 1)
                        {
                            activeBitmapCodes[BitmapCode.FrightenedGhost] = bitmapStateValCnt - 1;
                        }
                        else
                        {
                            activeBitmapCodes.Remove(BitmapCode.FrightenedGhost);
                        }
                    }
                    else
                    {
                        Debug.Log("Error - trying to remove non-existing pellet state");
                    }

                    GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, currentMaxBitmapCode);
                    return;
                }
            }
            else
            {
                if (activeBitmapCodes.Contains(BitmapCode.Ghost))
                {
                    activeBitmapCodes[BitmapCode.Ghost] = (int)activeBitmapCodes[BitmapCode.Ghost] - 1;
                }
                else
                {
                    Debug.Log("Error - trying to remove non-existing pellet state");
                }

                GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, currentMaxBitmapCode);
                return;
            }
        }
        // Update bitmap with pacman code
        else if (other.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (activeBitmapCodes.Contains(BitmapCode.Pacman))
            {
                activeBitmapCodes.Remove(BitmapCode.Pacman);
            }
            else
            {
                Debug.Log("Error - trying to remove non-existing pellet state");
            }

            GameManager.gm.stateRepresentation.UpdateStateValue(currentLocation, currentMaxBitmapCode);
            return;
        }
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
        eaten = false;
        activeBitmapCodes.Clear();
        RegisterWithBitmap();
    }

}
