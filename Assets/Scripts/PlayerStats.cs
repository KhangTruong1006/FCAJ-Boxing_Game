using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CharacterObject character;
    private GameManager gameManager;

    public float health;
    public float stamina;
    public float speed;

    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public bool isDucking;
    public bool isBlocking;
    public bool isAttacking;

    public bool isExhausted;

    private void Awake()
    {
        health = character.health;
        stamina = character.stamina;
        speed = character.speed;

        attackDamage = character.attackDamage;
        attackSpeed = character.attackSpeed;
        attackRange = character.attackRange;

        gameManager = Object.FindAnyObjectByType<GameManager>();
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
