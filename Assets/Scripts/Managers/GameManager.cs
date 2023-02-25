using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void PelletEatenEvent();
    public delegate void PowerPelletEatenEvent();
    public delegate void PowerPelletWornOff();
    public delegate void LostLife();
    public delegate void GhostEatenEvent();
    public delegate void RoundWonEvent();

    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public Text gameOverText;
    public Text scoreText;
    public Text livesText;

    [Min(1)]
    public int numLives;

    public static GameManager gm { get; private set; }
    public PelletEatenEvent pelletEatenEvent;
    public PowerPelletEatenEvent powerPelletEatenEvent;
    public PowerPelletWornOff powerPelletWornOff;
    public LostLife lostLife;
    public GhostEatenEvent ghostEatenEvent;
    public RoundWonEvent roundWonEvent;
    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }
    public bool infiniteLives = false;
    public Hashtable activePelletLocations { get; private set; }
    [HideInInspector]
    public StateRepresentation stateRepresentation;

    [SerializeField]
    private bool resetScoreOnNewRound = false;
    private bool firstLaunch = true;

    private void Awake()
    {
        gm = this;
        stateRepresentation = GetComponent<StateRepresentation>();
    }

    private void Start()
    {
        activePelletLocations = new Hashtable();

        NewGame();
    }

    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(numLives);
        NewRound();
    }

    private void NewRound()
    {
        if (resetScoreOnNewRound)
        {
            SetScore(0);
        }

        gameOverText.enabled = false;

        foreach (Transform pellet in pellets) {
            pellet.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            pellet.GetComponent<Pellet>().Reset();
            activePelletLocations.Add(pellet.position, "");
        }

        ResetState();
    }

    public void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        pacman.ResetState();
    }

    public void BroadcastPowerPelletWornOff()
    {
        powerPelletWornOff?.Invoke();
    }

    private void GameOver()
    {
        gameOverText.enabled = true;

        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        pacman.DeathSequence();

        lostLife?.Invoke();

        if (!infiniteLives)
        {
            SetLives(lives - 1);
        }

        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        ghostEatenEvent?.Invoke();

        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    // @triggerPelletEvent - toggle whether we want pellet eaten event to fire 
    // (used when power pellet consumed and we don't want double events)
    public void PelletEaten(Pellet pellet, bool triggerPelletEvent = true)
    {
        if (triggerPelletEvent)
        {
            pelletEatenEvent?.Invoke();
        }

        // pellet.gameObject.SetActive(false);
        pellet.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        activePelletLocations.Remove(pellet.transform.position);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            roundWonEvent?.Invoke();

            // Remove pellet from hashtable
            activePelletLocations.Remove(pellet.transform.position);

            
            pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        powerPelletEatenEvent?.Invoke();

        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet, false);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.GetComponent<SpriteRenderer>().enabled) {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

}
