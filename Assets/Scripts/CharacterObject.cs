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
    public float attackRange;
    public float attackSpeed; // Cooldown between attacks
}
