using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Setting settings;

    public float timer;
    public bool isTimerRunningOut = false;
    public bool isGameEnded = false;

    [Header("Player 1 Stats")]
    public PlayerStats player1Stats;


    [Header("Player 2 Stats")]
    public PlayerStats player2Stats;

    void Start()
    {
        timer = settings.match.duration;
    }

    // Update is called once per frame
    void Update()
    {
        runTimer();
        //if (!isGameEnded)
        //{
        //    runTimer();
        //}
    }

    private void runTimer()
    {
        if(!isTimerRunningOut)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }

            else
            {
                timer = 0f;
                isTimerRunningOut = true;
                isGameEnded = true;
                endGame();
            }
        } 
    }
    public void endGame()
    {
        Debug.Log("Game Over");
    }

    public void ProcessHitDetection(PlayerStats target, float damage)
    {
        if (target != null) {
            if (target.isBlocking || target.isDucking)
            {
                damage = 0f;
                Debug.Log("Player is blocking/ducking");
            }
        }
        
        target.takenDamage(damage);
    }

    // Will implement later lol
    private void updateHealthUI()
    {

    }
}
