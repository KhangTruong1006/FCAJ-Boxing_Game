using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float timer = 5f;
    public bool isTimerRunningOut = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    private void runTimer()
    {
        if(!isTimerRunningOut)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }

            else
            {
                timer = 0f;
                isTimerRunningOut = true;
            }
        } 
    }
    private void endGame()
    {
        Debug.Log("Game Over");
    }
}
