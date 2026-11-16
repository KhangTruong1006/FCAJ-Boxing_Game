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
}
