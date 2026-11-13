using UnityEngine;

[CreateAssetMenu(fileName = "Setting Object", menuName = "Scriptable Object / Setting")]
public class Setting : ScriptableObject
{
    public Punch punch;
    public Duck duck;

    [System.Serializable]
    public class Punch
    {
        [Min(1f)] public float mediumMultiplier = 1.2f;
        [Min(1f)] public float heavyMultiplier = 1.5f;
    }

    [System.Serializable]
    public class Duck
    {
        public float duration = 0.1f;
    }
}
