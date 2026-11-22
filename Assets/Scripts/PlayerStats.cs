using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CharacterObject character;
    private GameManager gameManager;

    public float health;
    public float speed;

    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public float maxStamina;
    public float currentStamina;

    private float regenDelayTimer;
    private float staminaRegenCooldown;
    private float staminaRegenRate;
    [Range(0.1f, 1f)]private float exhaustionThreshold;

    public bool isDucking;
    public bool isBlocking;
    public bool isAttacking;

    public bool isExhausted;

    private void Awake()
    {
        assignStatsFromCharacter();

        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.isGameEndedOrGracePeriod())
        {
            return;
        }
        
        handleUpdateStamina();
    }

    private void assignStatsFromCharacter()
    {
        if (character != null)
        {
            health = character.health;
            speed = character.speed;

            attackDamage = character.attackDamage;
            attackSpeed = character.attackSpeed;
            attackRange = character.attackRange;

            maxStamina = character.stamina;
            

            staminaRegenCooldown = character.staminaRegenCooldown;
            staminaRegenRate = character.staminaRegenRate;
            exhaustionThreshold = character.exhaustionThreshold;
        }

        else
        {
            health = 100f;
            speed = 5f;
            attackDamage = 10f;
            attackSpeed = 0.2f;
            attackRange = 0.5f;
            maxStamina = 100f;
            staminaRegenCooldown = 1f;
            staminaRegenRate = 10f;
            exhaustionThreshold = 0.3f;
        }

        currentStamina = maxStamina;
    }

    public void takenDamage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
           health = 0;
           gameManager.endGame();
        }
    }

    // ===== Stamina =====
    private void handleUpdateStamina()
    {
        float previousStamina = currentStamina;
        updateStamina(Time.deltaTime);

        if (gameManager != null && !Mathf.Approximately(previousStamina, currentStamina))
        {
            gameManager.onStaminaChange(this);
        }
    }

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

        currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * multiplier * deltaTime);

        if (isExhausted && currentStamina >= maxStamina * exhaustionThreshold)
        {
            isExhausted = false;
        }
    }

    public bool consumeStamina(float amount)
    {
        if (isExhausted || currentStamina < amount)
        {
            return false;
        }

        currentStamina = Mathf.Max(0f, currentStamina - amount);
        regenDelayTimer = staminaRegenCooldown;

        if (currentStamina <= 0f)
        {
            isExhausted = true;
        }

        if (gameManager != null)
        {
            gameManager.onStaminaChange(this);
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
}
