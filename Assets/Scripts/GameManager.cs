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

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    public NetworkVariable<float> syncedTimer = new NetworkVariable<float>(90f);
    public NetworkVariable<float> syncedGraceTimer = new NetworkVariable<float>(5f);

    public NetworkVariable<bool> isGameEnded = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isGracePeriod = new NetworkVariable<bool>(true);

    private readonly PlayerStats[] players = new PlayerStats[2];
    
    public NetworkVariable<int> registeredPlayerCount = new(0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            setSyncedGraceTimer();
            setSyncedTimer();

            isGameEnded.Value = false;
            isGracePeriod.Value = true;     
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned)
        {
            return;
        }

        if (ui != null)
        {
            if (isGracePeriod.Value && !areBothPlayersRegistered())
            {
                ui.displayWaitingForPlayers();
            }

            else if (isGracePeriod.Value)
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
        if (!isGracePeriod.Value || !areBothPlayersRegistered())
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

        float health_1 = getPlayerHealth(0);
        float health_2 = getPlayerHealth(1);
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

        showResultClientRpc(message);
    }

    private float getPlayerHealth(int index) { 
        return players[index] != null ? players[index].Health.Value : 0f;
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
    // ===== Condition =====
    public bool isGameEndedOrGracePeriod()
    {
        return isGameEnded.Value || isGracePeriod.Value;
    }

    private bool areBothPlayersRegistered()
    {
        return registeredPlayerCount.Value == 2;
    }

    // ===== Helpers =====
    private void setSyncedGraceTimer()
    {
        syncedGraceTimer.Value = settings != null && settings.match != null ? settings.match.grace_period : 5f;
    }

    private void setSyncedTimer()
    {
        syncedTimer.Value = settings != null && settings.match != null ? settings.match.duration : 90f;
    }

    public Transform getSpawnPoint(int slot)
    {
        return slot == 0 ? player1SpawnPoint : player2SpawnPoint;
    }

    // ===== UI Synchronization =====
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

    // ===== Player Registration =====
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
        if (!IsServer ||slot < 0 || slot > 1)
        {
            return;
        }

        if (players[slot] != null) {
            return;
        }
        players[slot] = stats;

        UpdatePlayerCount();
    }
    private void UpdatePlayerCount()
    {
        int count = 0;

        foreach (PlayerStats player in players)
        {
            if (player != null)
            {
                count++;
            }
        }

        registeredPlayerCount.Value = count;
    }

    public void unregisterPlayer(int slot, PlayerStats stats)
    {
        if (!IsServer || slot < 0 || slot > 1)
        {
            return;
        }
        if (players[slot] != stats)
        {
            return;
        }

        players[slot] = null;

        UpdatePlayerCount();

        if (registeredPlayerCount.Value == 0)
        {
            resetMatch();
            return;
        }

        if (isGameEnded.Value)
        {
            return;
        }

        if (isGracePeriod.Value)
        {
            setSyncedGraceTimer();
        }
        else
        {
            int winnerSlot = slot == 0 ? 1 : 0;

            isGameEnded.Value = true;

            showResultClientRpc(
                $"Player {winnerSlot + 1} Won\nOpponent disconnected."
            );
        }
    }

    private void resetMatch()
    {
        if (!IsServer)
        {
            return;
        }

        isGameEnded.Value = false;
        isGracePeriod.Value = true;

        setSyncedGraceTimer();
        setSyncedTimer();
    }
    public void configurePlayer(PlayerController controller, int slot)
    {
        if (controller == null || slot < 0 || slot > 1)
        {
            return;
        }

        if (slot == 0)
        {
            controller.SetSelfLayer(player1LayerIndex);
            controller.SetOpponentLayer(1 << player2LayerIndex);
            controller.setPlayerDirection(1f);
        }
        else
        {
            controller.SetSelfLayer(player2LayerIndex);
            controller.SetOpponentLayer(1 << player1LayerIndex);
            controller.setPlayerDirection(-1f);
        }
    }
}
