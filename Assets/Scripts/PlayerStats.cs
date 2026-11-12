using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CharacterObject character;

    public float health;
    public float stamina;
    public float speed;

    void Start()
    {
        health = character.health;
        stamina = character.stamina;
        speed = character.speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void takenDamanage(int damage)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            // Handle player death & Send update back to the game manager / server
            Debug.Log("Player has died.");
        }
    }
}
