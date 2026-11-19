using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] public Slider healthSlider1, staminaSlider1;
    [SerializeField] public Slider healthSlider2, staminaSlider2;
    [SerializeField] public GameObject resultPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI timerText, resultText;

    private void Start()
    {
        // Player 1 Stats - Healht & Stamina
        displaySliderValueUponStart(healthSlider1, gameManager.player1Stats.health);
        displaySliderValueUponStart(staminaSlider1, gameManager.player1Stats.stamina);

        // Player 2 Stats - Healht & Stamina
        displaySliderValueUponStart(healthSlider2, gameManager.player2Stats.health);
        displaySliderValueUponStart(staminaSlider2, gameManager.player2Stats.stamina);
    }

    private void displaySliderValueUponStart(Slider slider, float stat)
    {
        setSliderMaxValue(slider, stat);
        fillSliderBasedOnValue(slider, stat);
    }

    private void setSliderMaxValue(Slider slider, float maxValue)
    {
        slider.maxValue = maxValue;
    }

    public void fillSliderBasedOnValue(Slider slider, float value)
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
