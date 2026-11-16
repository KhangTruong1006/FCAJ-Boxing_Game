using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Setting settings;
    [SerializeField] private UIController ui;

    public float timer;
    public bool isTimerRunningOut = false;
    public bool isGameEnded = false;

    public bool isGracePeriod = true;
    public float graceTimer;

    [Header("Player 1 Stats")]
    public PlayerStats player1Stats;


    [Header("Player 2 Stats")]
    public PlayerStats player2Stats;

    void Start()
    {
        graceTimer = settings.match.grace_period;
        timer = settings.match.duration;  
    }

    // Update is called once per frame
    void Update()
    {
        runTimer();
    }

    private void runTimer()
    {
        if(!isTimerRunningOut)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                return;
            }

            else
            {
                timer = 0f;
                endGame();
            }
        } 
    }
    public void endGame()
    {
        isTimerRunningOut = true;
        isGameEnded = true;

        float health_1 = player1Stats.health;
        float health_2 = player2Stats.health;
        
        if (health_1 > health_2)
        {
            Debug.Log("Player 1 Won");
        }

        else if (health_1 < health_2)
        {
            Debug.Log("Player 2 Won");
        }

        else {
            Debug.Log("Tie");
        }
    }

    public void ProcessHit(PlayerStats target, float damage)
    {
        //if (isGracePeriod) {
        //    return;
        //}

        if (target != null) {
            if (target.isBlocking || target.isDucking)
            {
                damage = 0f;
            }
        }
        
        target.takenDamage(damage);
        updateHealthBar(target);
    }


    private void updateHealthBar(PlayerStats target)
    {
        if (target == player1Stats)
        {
            ui.fillHealthSliderBasedOnValue(ui.healthSlider1, target.health);
        }

        else if (target == player2Stats)
        {
            ui.fillHealthSliderBasedOnValue(ui.healthSlider2, target.health);
        }
    }
}
