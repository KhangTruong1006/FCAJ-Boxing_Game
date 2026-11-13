using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float timer = 5f;
    public bool isTimerRunningOut = false;

    [Header("Player 1 Stats")]
    public PlayerStats player1Stats;
    public bool isPlayer1Hit = false;


    [Header("Player 2 Stats")]
    public PlayerStats player2Stats;
    public bool isPlayer2Hit = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        target.takenDamage(damage);
    }
}
