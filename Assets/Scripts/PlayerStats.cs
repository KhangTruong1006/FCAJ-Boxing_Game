using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CharacterObject character;
    private GameManager gameManager;

    public float health;
    public float stamina;
    public float speed;

    public bool isDucking;
    public bool isBlocking;
    private bool isAttacking;

    void Start()
    {
        health = character.health;
        stamina = character.stamina;
        speed = character.speed;

        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
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
