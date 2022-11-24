using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLPLanner : MonoBehaviour
{
    public LayerMask obstacleLayer;
    [Range(0,1)]
    public float alpha = 0.1f;
    [Range(0,1)]
    public float gamma = 0.9f;

    const int NUM_FEATURES = 6;

    private float[] weights = { 1f, 1f, 1f, 1f, 1f, 1f };

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
            // Get new position post action
            newPos = transform.position + new Vector3(action.x, action.y, 0);
            // Center new position in square
            newPos.x = Mathf.Floor(newPos.x) + Mathf.Ceil(newPos.x) / 2;
            newPos.y = Mathf.Floor(newPos.y) + Mathf.Ceil(newPos.y) / 2;
            // Add to positions
            newPositions.Add(newPos);
        }

        // Get the state with best features
        int winningActionIdx = 0;
        float bestVal = 0f;
        float currentStateVal = GetCurrentStateVal(transform.position);
        float[] bestFeatures = GetHighestValueAction(newPositions, ref bestVal, ref winningActionIdx);

        /* Do Bellman Ford Equation update to weights */
        float diffVal = (1 + gamma * bestVal) - currentStateVal;

        int count = 0;
        
        // Update weights
        foreach (float feature in bestFeatures)
        {
            weights[count] += alpha * diffVal * feature;
        }

        // Return best action
        return actions[winningActionIdx];
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

    public float GetCurrentStateVal(Vector3 position)
    {
        Ghost[] ghosts = GameManager.gm.ghosts;

        float[] features = new float[NUM_FEATURES]; 

        features[0] = DistToClosestDot(position);
        features[1] = DistToClosestGhost(ghosts, position);
        features[2] = NumAliveGhosts(ghosts, position);
        features[3] = isPacmanInTunnel(position);
        features[4] = NumChasingGhosts(ghosts);
        features[5] = PowerPelletEaten(ghosts, position);

        return weights[0] * features[0] + weights[1] * features[1] + weights[2] * features[2]
                + weights[3] * features[3] + weights[4] * features[4] + weights[5] * features[5];
    }

    public float[] GetHighestValueAction(List<Vector3> positions, ref float bestVal, ref int winningActionIdx)
    {
        winningActionIdx = 0;
        int currentActionIdx = 0;

        float highestValue = float.MinValue;
        float currentVal = 0;
        Ghost[] ghosts = GameManager.gm.ghosts;
        float[] bestFeatures = new float[NUM_FEATURES];
        float[] features = new float[NUM_FEATURES];

        foreach (Vector3 pos in positions)
        {
            features[0] = DistToClosestDot(pos);
            features[1] = DistToClosestGhost(ghosts, pos);
            features[2] = NumAliveGhosts(ghosts, pos);
            features[3] = isPacmanInTunnel(pos);
            features[4] = NumChasingGhosts(ghosts);
            features[5] = PowerPelletEaten(ghosts, pos);

            currentVal = weights[0] * features[0] + weights[1] * features[1] + weights[2] * features[2]
                        + weights[3] * features[3] + weights[4] * features[4] + weights[5] * features[5];

            if (currentVal > highestValue)
            {
                bestFeatures = features;
                winningActionIdx = currentActionIdx;
            }

            currentActionIdx++;
        }

        bestVal = highestValue;
        return bestFeatures;
    }

    /*
     * Feature functions below
     */

     // Get distance to closest dot - does not check if there are no dots left
     public float DistToClosestDot(Vector3 state)
    {
        float closestDist = float.MaxValue;
        float dist = 0;

        foreach (Transform child in GameManager.gm.pellets.transform)
        {
            dist = Vector3.Distance(state, child.position);
            // float dist = Vector2.Distance(new Vector2(child.position.x, child.position.y), new Vector2(child.position.x, child.position.y));
            if (dist < closestDist)
            {
                closestDist = dist;
            }
        }

        return closestDist;
    }

    public float DistToClosestGhost(Ghost[] ghosts, Vector3 state)
    {
        float closestDist = float.MaxValue;
        float currentDist = 0;

        foreach (Ghost ghost in ghosts)
        {
            currentDist = Vector3.Distance(state, ghost.transform.position);
            // float dist = Vector2.Distance(new Vector2(child.position.x, child.position.y), new Vector2(child.position.x, child.position.y));
            if (currentDist < closestDist)
            {
                closestDist = currentDist;
            }
        }

        return closestDist;
    }

    public int NumAliveGhosts(Ghost[] ghosts, Vector3 state)
    {
        int numAliveGhosts = 0;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost.home.enabled == true)
            {
                continue;
            }

            numAliveGhosts++;
        }

        return numAliveGhosts;
    }

    public int isPacmanInTunnel(Vector3 state)
    {
        Pacman pacman = GameManager.gm.pacman;
        Vector3 currentPos = pacman.transform.position;
        RaycastHit2D rightHit = Physics2D.BoxCast(state, Vector2.one * 0.5f, 0f, Vector2.right, 1f, obstacleLayer);
        RaycastHit2D leftHit = Physics2D.BoxCast(state, Vector2.one * 0.5f, 0f, Vector2.left, 1f, obstacleLayer);

        bool hasRightHit = rightHit.transform.gameObject.GetComponent<Passage>() != null ? true : false;
        bool hasLeftHit = leftHit.transform.gameObject.GetComponent<Passage>() != null ? true : false;

        // If we have a right hit and are within the tunnel
        if (hasRightHit && state.x > rightHit.transform.position.x)
        {
            return 1;
        }

        // If we have a left hit and are within the tunnel
        if (hasLeftHit && state.x < leftHit.transform.position.x)
        {
            return 1;
        }

        return 0;
    }

    public int NumChasingGhosts(Ghost[] ghosts)
    {
        int numChasingGhosts = 0;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost.chase.enabled == false)
            {
                continue;
            }

            numChasingGhosts++;
        }

        return numChasingGhosts;
    }

    public int PowerPelletEaten(Ghost[] ghosts, Vector3 state)
    {
        // Check if we already have a pellet eaten
        if (ghosts[0].frightened == enabled)
        {
            return 1;
        }

        List<Vector2> powerpellets = GameManager.gm.powerPelletPositions;
        Vector2 xBounds;

        // Check if we will have eaten a power pellet
        foreach (Vector2 powerpellet in powerpellets)
        {
            xBounds = new Vector2(powerpellet.x - 0.5f, powerpellet.x + 0.5f);
            if (state.x > xBounds.x && state.x < xBounds.y)
            {
                return 1;
            }
        }

        return 0;
    }
}
