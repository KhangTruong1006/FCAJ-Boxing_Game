using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private CharacterObject character;
    private GameManager gameManager;

    public float speed;

    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100f);
    public NetworkVariable<float> Health = new NetworkVariable<float>(100f);
    public NetworkVariable<float> MaxStamina = new NetworkVariable<float>(100f);
    public NetworkVariable<float> CurrentStamina = new NetworkVariable<float>(100f);

    private float regenDelayTimer;
    private float staminaRegenCooldown;
    private float staminaRegenRate;
    [Range(0.1f, 1f)]private float exhaustionThreshold;

    public bool isDucking { get; private set; }
    public bool isBlocking { get; private set; }
    public bool isAttacking { get; private set; }
    public bool isExhausted { get; private set; }

    public NetworkVariable<int> SyncedSlot = new(-1);

    public int playerSlot => SyncedSlot.Value;

    private void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
        assignLocalStatsFromCharacter();       
    }

    public override void OnNetworkSpawn()
    {
        if(gameManager == null)
        {
            gameManager = Object.FindAnyObjectByType<GameManager>();
        }

        if (IsServer)
        {
            NetworkSession session = NetworkManager.GetComponent<NetworkSession>();

            SyncedSlot.Value = session.getReservedSlot(OwnerClientId);

            MaxHealth.Value = character != null ? character.health : 100f;
            MaxStamina.Value = character != null ? character.stamina : 100f;

            Health.Value = MaxHealth.Value;
            CurrentStamina.Value = MaxStamina.Value;

            if (gameManager != null)
            {
                gameManager.registerPlayer(playerSlot, this);
            }
        }

        Health.OnValueChanged += handleHealthChanged;
        CurrentStamina.OnValueChanged += handleStaminaChanged;
        SyncedSlot.OnValueChanged += handleSlotChanged;

        RefreshStatsUI();
    }

    public override void OnNetworkDespawn()
    {
        Health.OnValueChanged -= handleHealthChanged;
        CurrentStamina.OnValueChanged -= handleStaminaChanged;
        SyncedSlot.OnValueChanged -= handleSlotChanged;

        if (IsServer && gameManager != null && gameManager.IsSpawned && !NetworkManager.ShutdownInProgress)
        {
            gameManager.unregisterPlayer(playerSlot, this);
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        
        if (gameManager != null && gameManager.isGameEndedOrGracePeriod())
        {
            return;
        }
        
        updateStamina(Time.deltaTime);
    }

    private void assignLocalStatsFromCharacter()
    {
        if (character != null)
        {

            speed = character.speed;

            attackDamage = character.attackDamage;
            attackSpeed = character.attackSpeed;
            attackRange = character.attackRange;

            staminaRegenCooldown = character.staminaRegenCooldown;
            staminaRegenRate = character.staminaRegenRate;
            exhaustionThreshold = character.exhaustionThreshold;
        }

        else
        {
            speed = 5f;

            attackDamage = 10f;
            attackSpeed = 0.2f;
            attackRange = 0.5f;
;
            staminaRegenCooldown = 1f;
            staminaRegenRate = 10f;
            exhaustionThreshold = 0.3f;
        }
    }

    public void takenDamage(float damage)
    {
        if(!IsServer) 
        { 
            return; 
        }
        
        Health.Value = Mathf.Max(0f, Health.Value - damage);
        if(Health.Value <= 0 && gameManager != null)
        {
           gameManager.endGame();
        }
    }

    // ===== Stamina =====
    private void updateStamina(float deltaTime)
    {
        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= deltaTime;
            return;
        }

        float multiplier = getStaminaRegenMultiplier();
        
        if (multiplier <= 0f)
        {
            return;
        }

        CurrentStamina.Value = Mathf.Min(MaxStamina.Value, CurrentStamina.Value + staminaRegenRate * multiplier * deltaTime);

        if (isExhausted && CurrentStamina.Value >= MaxStamina.Value * exhaustionThreshold)
        {
            isExhausted = false;
        }
    }

    public bool consumeStamina(float amount)
    {
        if (!IsServer || isExhausted || CurrentStamina.Value < amount)
        {
            return false;
        }

        CurrentStamina.Value = Mathf.Max(0f, CurrentStamina.Value - amount);
        regenDelayTimer = staminaRegenCooldown;

        if (CurrentStamina.Value <= 0f)
        {
            isExhausted = true;
        }

        return true;
    }

    private float getStaminaRegenMultiplier()
    {
        if (isAttacking)
        {
            return 0f;
        }

        if (isBlocking)
        { 
            return 0.25f; 
        }

        if (isDucking) 
        { 
            return 0.5f; 
        }

        return 1f; // Default
    }

    // ===== Conditions =====
    // Can either duck or block at a moment, not both
    public void setBlocking(bool blocking)
    {
        isBlocking = blocking;
        if (isBlocking)
        {
            isDucking = false;
        }
    }

    public void setDucking(bool ducking)
    {
        isDucking = ducking;
        if (isDucking)
        {
            isBlocking = false;
        }
    }

    public void setAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    // ===== UI =====
    private void handleHealthChanged(float previousValue, float newValue)
    {
        if (gameManager != null)
        {
            gameManager.syncHealthUI(playerSlot, newValue, MaxHealth.Value);
        }
    }

    private void handleStaminaChanged(float previousValue, float newValue)
    {
        if (gameManager != null)
        {
            gameManager.syncStaminaUI(playerSlot, newValue, MaxStamina.Value);
        }
    }

    private void handleSlotChanged(int previousValue, int newValue)
    {
        RefreshStatsUI();
    }

    private void RefreshStatsUI()
    {
        if (gameManager == null || playerSlot < 0)
        {
            return;
        }

        gameManager.syncHealthUI(
            playerSlot,
            Health.Value,
            MaxHealth.Value
        );

        gameManager.syncStaminaUI(
            playerSlot,
            CurrentStamina.Value,
            MaxStamina.Value
        );
    }
}
