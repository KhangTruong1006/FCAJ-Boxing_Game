using UnityEngine;

[CreateAssetMenu(fileName = "Character Object", menuName = "Scriptable Object / Character")]
[System.Serializable]
public class CharacterObject : ScriptableObject
{
    [Header("Character Info")]
    public string characterID; // Remember to change to a unique ID when creating a new character
    public string characterName;

    [Header("Character Stats")]
    public float health;
    public float stamina;
    public float speed;

    public float attackDamage;
    [Range(0f, 1f)]
    public float attackRange;
    [Range(0f, 1f)]
    public float attackSpeed; // Delay between input and inflicting damage

    public float staminaRegenCooldown;
    public float staminaRegenRate = 5f;
    [Range(0.1f, 1f)] 
    public float exhaustionThreshold = 0.3f;
}
