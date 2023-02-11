using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[System.Serializable]
public struct RewardDataTemplate
{
    public float pelletReward;
    public float powerPelletReward;
    public float eatenReward;
    public float ghostEatenReward;
    public float winReward;
    public float lastPelletEatTimePenaltyThreshold;
    public float pelletEatTimePenalty;
}

public class PacmanAIController : Agent
{
    public Pacman pacman { get; private set; }
    public bool isAIControlled { get; private set; }
    public RewardDataTemplate rewardData;
    private RLPLanner rlplanner;
    private Dictionary<int, Vector2> actionMap;
    private float lastPelletEatTime = 0f;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        isAIControlled = pacman.isAIControlled;
        rlplanner = GetComponent<RLPLanner>();

        actionMap = new Dictionary<int, Vector2>();
        actionMap.Add(0, Vector2.up);
        actionMap.Add(1, Vector2.down);
        actionMap.Add(2, Vector2.right);
        actionMap.Add(3, Vector2.left);

        //Subscribe to GameManager events
        GameManager.gm.pelletEatenEvent = () =>
        {
            SetReward(rewardData.pelletReward);
        };

        GameManager.gm.lostLife = () =>
        {
            SetReward(rewardData.eatenReward);
            lastPelletEatTime = Time.time;
            Debug.Log("Lost Life - rewarded " + rewardData.eatenReward);
        };

        GameManager.gm.powerPelletEatenEvent = () =>
        {
            SetReward(rewardData.powerPelletReward);
            lastPelletEatTime = Time.time;
        };  

        GameManager.gm.ghostEatenEvent = () =>
        {
            SetReward(rewardData.ghostEatenReward);
        };

        GameManager.gm.roundWonEvent = () =>
        {
            SetReward(rewardData.winReward);
        };

        InvokeRepeating(nameof(CheckLastPelletEatTime), 0, rewardData.lastPelletEatTimePenaltyThreshold);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        // Reset characters to initial positions on new episode
        GameManager.gm.ResetState();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[] features = rlplanner.GetDNNStateRepresentation(pacman.gameObject.transform.position);
        foreach (float feature in features)
        {
            sensor.AddObservation(feature);
        }

        Debug.Log("Added features");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        Debug.Log("ActionAmt:" + discreteActions[0]);

        pacman.Move(actionMap[discreteActions[0]]);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAIControlled)
        {
            Vector2 location = new Vector2(other.bounds.center.x, other.bounds.center.y);
            Vector2 chosenAction = rlplanner.ChooseNewAction(location);
            pacman.Move(chosenAction);
        }
    }

    private Vector2 ChooseAction(List<Vector2> actions)
    {
        int numActions = actions.Count;
        int actionIdx = Random.Range(0, numActions - 1);
        return actions[actionIdx];
    }

    // Gives penalty for not eating pellets for too long
    // Also resets lastPelletEatTime to currentTime if above time threshold
    private void CheckLastPelletEatTime()
    {
        float currentTime = Time.time;
        float timeDiff = currentTime - lastPelletEatTime;
        if (timeDiff > rewardData.lastPelletEatTimePenaltyThreshold)
        {
            SetReward(rewardData.pelletEatTimePenalty);
            Debug.Log("Last pellet eaten penalty " + rewardData.pelletEatTimePenalty);
            lastPelletEatTime = currentTime;
        }
    }
}
