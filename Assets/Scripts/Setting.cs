using UnityEngine;

[CreateAssetMenu(fileName = "Setting Object", menuName = "Scriptable Object / Setting")]
public class Setting : ScriptableObject
{
    public Punch punch;
    public Duck duck;
    public Match match;

    [System.Serializable]
    public class Match
    {
        public float duration = 90f;
    }
     
    [System.Serializable]
    public class Punch
    {
        [Min(1f)] public float defaultMultiplier = 1f;
        [Min(1f)] public float mediumMultiplier = 1.2f;
        [Min(1f)] public float heavyMultiplier = 1.5f;
    }

    [System.Serializable]
    public class Duck
    {
        public float duration = 0.1f;
    }
}
