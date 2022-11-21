using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacmanAIController : MonoBehaviour
{
    public Pacman pacman { get; private set; }
    public bool isAIControlled { get; private set; }
    private RLPLanner rlplanner;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        isAIControlled = pacman.isAIControlled;
        rlplanner = GetComponent<RLPLanner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAIControlled)
        {
            Vector2 location = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y);
            Vector2 chosenAction = rlplanner.ChooseNewAction(location);
        }
    }

    private Vector2 ChooseAction(List<Vector2> actions)
    {
        int numActions = actions.Count;
        int actionIdx = Random.Range(0, numActions - 1);
        return actions[actionIdx];
    }
}
