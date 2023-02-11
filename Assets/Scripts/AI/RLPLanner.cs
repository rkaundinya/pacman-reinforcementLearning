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
    [Range(0,1000)]
    public int featureMax = 10;
    [Range(0,1000)]
    public int weightMax = 10;
    [Range(-10, 10)]
    public float livingReward = 0;
    [Range(0,100)]
    public float pelletReward = 5;
    [Range(-100, 0)]
    public float hitGhostReward = -50;
    [Min(0)]
    public int epsilonGreedy = 3;
    [Min(1)]
    public float epsilonUpdateTime = 2;

    const int NUM_FEATURES = 6;

    public float[] weights = { 1f, 1f, 1f, 1f, 1f, 1f };

    private Hashtable visitedPositions;
    private bool powerPelletEaten = false;


    public void Start()
    {
        visitedPositions = new Hashtable();
        GameManager.gm.powerPelletEatenEvent = () =>
        {
            powerPelletEaten = true;
        };

        GameManager.gm.powerPelletWornOff = () =>
        {
            powerPelletEaten = false;
        };
        // InvokeRepeating("UpdateEpsilonGreedy", 2f, epsilonUpdateTime);
    }

    public void UpdateEpsilonGreedy()
    {
        epsilonGreedy++;
    }

    // Used by game systems to update reward of planner for given state --- e.g. on death and win 
    public void UpdateWithReward(Vector3 position, float reward)
    {
        // float currentStateVal = GetCurrentStateVal(position);
    }

    // Assumes input of centered location
    public Vector2 ChooseNewAction(Vector2 currentLocation)
    {
        if (!visitedPositions.ContainsKey(currentLocation))
        {
            visitedPositions.Add(currentLocation, 1f);
        }
        else
        {
            float currentVal = (float)visitedPositions[currentLocation] + 1f;
            visitedPositions[currentLocation] = currentVal;
        }
        //int numActions = currentNode.availableDirections.Count;
        //int actionIdx = Random.Range(0, numActions - 1);
        List<Vector2> actions = GetAvailableActions(currentLocation);
        List<Vector3> newPositions = new List<Vector3>();

        Vector3 newPos = Vector3.zero;

        foreach (Vector2 action in actions)
        {
            // Get new position post action
            newPos = new Vector3(currentLocation.x, currentLocation.y, transform.position.z) + new Vector3(action.x, action.y, 0);
            // Center new position in square
            /*newPos.x = Mathf.Floor(newPos.x) + Mathf.Ceil(newPos.x) / 2;
            newPos.y = Mathf.Floor(newPos.y) + Mathf.Ceil(newPos.y) / 2;*/
            // Add to positions
            newPositions.Add(newPos);
        }

        // Get the state with best features
        int winningActionIdx = 0;
        float bestVal = 0f;
        float currentStateVal = GetCurrentStateVal(currentLocation);
        float reward = GetReward(currentLocation, GameManager.gm.ghosts);
        float[] bestFeatures;
        // Check if we should take a random action
        float randVal = Random.Range(0.0f, 1.0f);
        if (randVal > 1.0f/ (float)visitedPositions[currentLocation])
        {
            bestFeatures = GetHighestValueAction(newPositions, ref bestVal, ref winningActionIdx);
        }
        else
        {
            // TODO - replace this with an overloaded GetHighestValueAction which takes in a single vector3 instead of a list to simplify
            int randPosIdx = Random.Range(0, newPositions.Count - 1);
            Vector3 randPos = newPositions[randPosIdx];
            newPositions.Clear();
            newPositions.Add(randPos);
            bestFeatures = GetHighestValueAction(newPositions, ref bestVal, ref winningActionIdx);
            winningActionIdx = randPosIdx;
        }

        /* Do Bellman Ford Equation update to weights */
        float diffVal = alpha * (reward + gamma * bestVal) - currentStateVal;

        int count = 0;
        float newWeightVal = 0;
        
        // Update weights
        foreach (float feature in bestFeatures)
        {
            newWeightVal = Mathf.Max(-100, weights[count] + (alpha * diffVal * feature));
            weights[count] =  newWeightVal > weightMax ? weightMax : newWeightVal;
            
            // Debug.Log("Updated weight to " + weights[count]);
            count++;
        }

        // Return best action
        return actions.Count > 0 ? actions[winningActionIdx] : Vector2.zero;
    }

    // Assumes getting centered state
    public float GetReward(Vector3 state, Ghost[] ghosts)
    {
        float centeredX = 0;
        float centeredY = 0;
        foreach (Ghost ghost in ghosts)
        {
            centeredX = (Mathf.Ceil(ghost.transform.position.x) + Mathf.Floor(ghost.transform.position.x)) / 2;
            centeredY = (Mathf.Ceil(ghost.transform.position.y) + Mathf.Floor(ghost.transform.position.y)) / 2;
            if (state.x == centeredX && state.y == centeredY)
            {
                return hitGhostReward;
            }
        }

        if (GameManager.gm.activePelletLocations.ContainsKey(state))
        {
            return pelletReward;
        }

        return livingReward;
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
            RaycastHit2D hit = Physics2D.BoxCast(currentLocation, Vector2.one * 0.5f, 0f, direction, 1f, obstacleLayer);

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

    public float[] GetDNNStateRepresentation(Vector3 position)
    {
        Ghost[] ghosts = GameManager.gm.ghosts;

        float[] features = new float[NUM_FEATURES];

        features[0] = DistToClosestDot(position);
        features[1] = DistToClosestGhost(ghosts, position);
        features[2] = NumAliveGhosts(ghosts, position);
        features[3] = isPacmanInTunnel(position);
        features[4] = NumChasingGhosts(ghosts);
        features[5] = PowerPelletEaten(ghosts, position);

        return features;
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
                highestValue = currentVal;
            }

            currentActionIdx++;
        }

        bestVal = highestValue;
        return bestFeatures;
    }

    /*
     * Feature functions below
     * TODO - Add a state has pellet feature
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

        // return Mathf.Max(0,closestDist) > featureMax ? featureMax : closestDist;
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

        // return Mathf.Max(0, closestDist) > featureMax ? featureMax : closestDist;
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

        bool hasRightHit = rightHit.collider != null && rightHit.transform.gameObject.GetComponent<Passage>() != null ? true : false;
        bool hasLeftHit = leftHit.collider != null && leftHit.transform.gameObject.GetComponent<Passage>() != null ? true : false;

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
        return powerPelletEaten ? 1 : 0;
    }
}
