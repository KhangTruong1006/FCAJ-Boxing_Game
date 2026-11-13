using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CharacterObject character;
    private GameManager gameManager;

    public float health;
    public float stamina;
    public float speed;
    public float attackDamage;

    public bool isDucking;
    public bool isBlocking;
    private bool isAttacking;

    private void Awake()
    {
        health = character.health;
        stamina = character.stamina;
        speed = character.speed;

        attackDamage = character.attackDamage;

        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    void Start()
    {
        
    }

    public void takenDamage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            gameManager.endGame();
            Debug.Log("Player has died.");
        }
    }
}
