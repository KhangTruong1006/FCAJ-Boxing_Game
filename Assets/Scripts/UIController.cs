using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] public Slider healthSlider1, healthSlider2;
    [SerializeField] public GameObject resultPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI timerText, resultText;

    private void Start()
    {
        displayHealthUponStart(healthSlider1, gameManager.player1Stats.health);
        displayHealthUponStart(healthSlider2, gameManager.player2Stats.health);
    }

    private void displayHealthUponStart(Slider slider, float health)
    {
        setSliderMaxValue(slider, health);
        fillHealthSliderBasedOnValue(slider, health);
    }

    private void setSliderMaxValue(Slider slider, float maxValue)
    {
        slider.maxValue = maxValue;
    }

    public void fillHealthSliderBasedOnValue(Slider slider, float value)
    {
        slider.value = value;
    }

    public void displayTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void displayResult(string message)
    {
        if (resultText != null) {
            resultText.text = message;
            resultPanel.SetActive(true);
        }
    }
}
