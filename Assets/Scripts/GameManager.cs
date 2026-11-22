using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Setting settings;
    [SerializeField] private UIController ui;

    public float timer;
    public bool isGameEnded = false;

    public float graceTimer;
    public bool isGracePeriod = true;

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
        runGraceTimer();

        if (!isGracePeriod)
        {
            runTimer();
        }
    }

    private void runGraceTimer()
    {
        if (isGracePeriod)
        {
            if (graceTimer > 0f)
            {
                graceTimer -= Time.deltaTime;

            }
            else
            {
                graceTimer = 0f;
                isGracePeriod = false;
            }
            ui.displayTimer(graceTimer);
        }
    }

    private void runTimer()
    {
        if (!isGameEnded)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }

            else
            {
                timer = 0f;
                endGame();
            }

            ui.displayTimer(timer);
        }
    }
    public void endGame()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        float health_1 = player1Stats != null ? player1Stats.health : 0f;
        float health_2 = player2Stats != null ? player2Stats.health : 0f;
        string message = "";

        if (health_1 > health_2)
        {
            message = "Player 1 Won";
        }

        else if (health_1 < health_2)
        {
            message = "Player 2 Won";
        }

        else
        {
            message = "Tie";
        }

        if (ui != null)
        {
            ui.displayResult(message);
        }
    }

    public void ProcessHit(PlayerStats target, float damage)
    {
        if (isGracePeriod || isGameEnded || target == null)
        {
            return;
        }

        if (target.isBlocking || target.isDucking)
        {
            damage = 0f;
        }

        target.takenDamage(damage);
        updateHealthBar(target);
    }

    public bool isGameEndedOrGracePeriod()
    {
        return isGameEnded || isGracePeriod;
    }

    private void updateHealthBar(PlayerStats target)
    {
        if (target == player1Stats)
        {
            ui.fillSliderBasedOnValue(ui.healthSlider1, target.health);
        }

        else if (target == player2Stats)
        {
            ui.fillSliderBasedOnValue(ui.healthSlider2, target.health);
        }
    }

    public void onStaminaChange(PlayerStats source)
    {
        if (source == null)
        {
            return;
        }

        if (source == player1Stats)
        {
            ui.fillSliderBasedOnValue(ui.staminaSlider1, source.currentStamina);
        }
        else if (source == player2Stats)
        {
            ui.fillSliderBasedOnValue(ui.staminaSlider2, source.currentStamina);
        }
    }
}
