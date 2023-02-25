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
            Debug.Log("Pellet eaten reward " + rewardData.pelletReward);
        };

        GameManager.gm.lostLife = () =>
        {
            SetReward(rewardData.eatenReward);
            Debug.Log("Lost Life - rewarded " + rewardData.eatenReward);
        };

        GameManager.gm.powerPelletEatenEvent = () =>
        {
            SetReward(rewardData.powerPelletReward);
            Debug.Log("Power Pellet eaten reward " + rewardData.powerPelletReward);
        };  

        GameManager.gm.ghostEatenEvent = () =>
        {
            SetReward(rewardData.ghostEatenReward);
            Debug.Log("Ghost eaten reward - " + rewardData.ghostEatenReward);
        };

        GameManager.gm.roundWonEvent = () =>
        {
            SetReward(rewardData.winReward);
            Debug.Log("Round won reward " + rewardData.winReward);
        };
    }

    public override void OnEpisodeBegin()
    {
        // Reset characters to initial positions on new episode
        Debug.Log("Episode began");
        GameManager.gm.ResetState();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int[] bitmap = GameManager.gm.stateRepresentation.GetBitcodeMap();
        foreach (int bitCode in bitmap)
        {
            sensor.AddObservation(bitCode);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> discreteActions = actions.DiscreteActions;

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
}
