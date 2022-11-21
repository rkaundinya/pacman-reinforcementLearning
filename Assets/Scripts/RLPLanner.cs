using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLPLanner : MonoBehaviour
{
    public LayerMask obstacleLayer;
    private float[] weights = new float[2];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 ChooseNewAction(Vector2 currentLocation)
    {
        //int numActions = currentNode.availableDirections.Count;
        //int actionIdx = Random.Range(0, numActions - 1);
        List<Vector2> actions = GetAvailableActions(currentLocation);
        List<Vector3> newPositions = new List<Vector3>();

        Vector3 newPos = Vector3.zero;

        foreach (Vector2 action in actions)
        {
            newPos = transform.position + new Vector3(action.x, action.y, 0);
            newPos.x = Mathf.Floor(newPos.x) + Mathf.Ceil(newPos.x) / 2;
            newPos.y = Mathf.Floor(newPos.y) + Mathf.Ceil(newPos.y) / 2;
            newPositions.Add(newPos);
            Debug.Log(DistToClosestDot(newPos));
        }




        /*foreach (Vector2 direction in currentNode.availableDirections)
        {
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1f, obstacleLayer);
            // Gizmos.DrawCube(new Vector3(hit.centroid.x, hit.centroid.y, 0), new Vector3(1, 1, 1));
            Debug.Log(count);
        }*/

        return Vector2.zero;
    }

    public List<Vector2> GetAvailableActions(Vector2 currentLocation)
    {
        List<Vector2> directions = new List<Vector2>();
        List<Vector2> actions = new List<Vector2>();

        directions.Add(Vector2.up);
        directions.Add(Vector2.down);
        directions.Add(Vector2.left);
        directions.Add(Vector2.right);

        foreach (Vector2 direction in directions)
        {
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1f, obstacleLayer);

            if (hit.collider == null)
            {
                actions.Add(direction);
            }
        }

        return actions;
    }

    /*
     * Feature functions below
     */

     // Get distance to closest dot - does not check if there are no dots left
     public float DistToClosestDot(Vector3 state)
    {
        float closestDist = float.MaxValue;
        foreach (Transform child in GameManager.gm.pellets.transform)
        {
            float dist = Vector3.Distance(state, child.position);
            // float dist = Vector2.Distance(new Vector2(child.position.x, child.position.y), new Vector2(child.position.x, child.position.y));
            if (dist < closestDist)
            {
                closestDist = dist;
            }
        }

        return closestDist;
    }
}
