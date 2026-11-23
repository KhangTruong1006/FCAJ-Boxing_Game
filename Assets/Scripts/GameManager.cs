using System;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Setting settings;
    [SerializeField] private UIController ui;

    [Header("Physics Layers")]
    [SerializeField] private int player1LayerIndex = 6;
    [SerializeField] private int player2LayerIndex = 7;

    public NetworkVariable<float> syncedTimer = new NetworkVariable<float>(90f);
    public NetworkVariable<float> syncedGraceTimer = new NetworkVariable<float>(5f);

    public NetworkVariable<bool> isGameEnded = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isGracePeriod = new NetworkVariable<bool>(true);

    private readonly PlayerStats[] players = new PlayerStats[2];

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            syncedTimer.Value = settings != null && settings.match != null ? settings.match.duration : 90f;
            syncedGraceTimer.Value = settings != null && settings.match != null ? settings.match.grace_period : 5f;

            isGameEnded.Value = false;
            isGracePeriod.Value = true;     
        }
    }

    public int getNextFreeSlot()
    {
        if (players[0] == null)
        {
            return 0;
        }

        if (players[1] == null)
        {
            return 1;
        }

        return 0;
    }

    public void registerPlayer(int slot, PlayerStats stats)
    {
        if (slot < 0 || slot > 1) 
        { 
            return; 
        }

        players[slot] = stats;

        if (IsServer && players[0] != null && players[1] != null)
        {
            ConfigureBothPlayers();
        }
    }

    internal void unregisterPlayer(int slot, PlayerStats stats)
    {
        if (slot < 0 || slot > 1) { 
            return; 
        }
        if (players[slot] == stats)
        {
            players[slot] = null;
        }
    }
    private void ConfigureBothPlayers()
    {
        PlayerController p1Ctrl = players[0].GetComponent<PlayerController>();
        PlayerController p2Ctrl = players[1].GetComponent<PlayerController>();

        if (p1Ctrl != null)
        {
            p1Ctrl.configurePlayersDirectionRpc(player1LayerIndex, 1 << player2LayerIndex, 1f);
        }

        if (p2Ctrl != null)
        {
            p2Ctrl.configurePlayersDirectionRpc(player2LayerIndex, 1 << player1LayerIndex, -1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(ui != null)
        {
            if (isGracePeriod.Value)
            {
                ui.displayTimer(syncedGraceTimer.Value);
            }

            else 
            {
                ui.displayTimer(syncedTimer.Value);
            }
        }

        if (!IsServer)
        {
            return;
        }
        
        runGraceTimer();

        if (!isGracePeriod.Value)
        {
            runTimer();
        }
    }

    private void runGraceTimer()
    {
        if (!isGracePeriod.Value)
        {
            return;
        }

        if (syncedGraceTimer.Value > 0f)
        {
            syncedGraceTimer.Value = Mathf.Max(0f, syncedGraceTimer.Value - Time.deltaTime);
        }
        else
        {
            syncedGraceTimer.Value = 0f;
            isGracePeriod.Value = false;
        }
    }

    private void runTimer()
    {
        if (isGameEnded.Value) 
        { 
            return; 
        }

        if (syncedTimer.Value > 0f)
        {
            syncedTimer.Value = Mathf.Max(0f, syncedTimer.Value - Time.deltaTime);
        }
        else
        {
            syncedTimer.Value = 0f;
            endGame();
        }
    }
    public void endGame()
    {
        if (!IsServer || isGameEnded.Value)
        {
            return;
        }

        isGameEnded.Value = true;

        float health_1 = players[0] != null ? players[0].Health.Value : 0f;
        float health_2 = players[1] != null ? players[1].Health.Value : 0f;
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
            showResultClientRpc(message);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void showResultClientRpc(string message)
    {
        if (ui != null)
        {
            ui.displayResult(message);
        }
    }

    public void ProcessHit(PlayerStats target, float damage)
    {
        if (isGameEndedOrGracePeriod() || target == null)
        {
            return;
        }

        if (target.isBlocking || target.isDucking)
        {
            damage = 0f;
        }

        target.takenDamage(damage);
    }

    public bool isGameEndedOrGracePeriod()
    {
        return isGameEnded.Value || isGracePeriod.Value;
    }

    public void syncHealthUI(int slot, float currentHealth, float maxHealth)
    {
        if (ui == null) return;

        if (slot == 0 && ui.healthSlider1 != null)
        {
            ui.healthSlider1.maxValue = maxHealth;
            ui.fillSliderBasedOnValue(ui.healthSlider1, currentHealth);
        }
        else if (slot == 1 && ui.healthSlider2 != null)
        {  
            ui.healthSlider2.maxValue = maxHealth;
            ui.fillSliderBasedOnValue(ui.healthSlider2, currentHealth);
        }
    }

    public void syncStaminaUI(int slot, float currentStamina, float maxStamina)
    {
        if (ui == null) return;

        if (slot == 0 && ui.staminaSlider1 != null)
        {
            ui.staminaSlider1.maxValue = maxStamina;
            ui.fillSliderBasedOnValue(ui.staminaSlider1, currentStamina);
        }
        else if (slot == 1 && ui.staminaSlider2 != null)
        {
            ui.staminaSlider2.maxValue = maxStamina;
            ui.fillSliderBasedOnValue(ui.staminaSlider2, currentStamina);
        }
    }
}
