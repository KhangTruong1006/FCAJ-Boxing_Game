using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] public Slider healthSlider1, healthSlider2;
    [SerializeField] private GameManager gameManager;

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
}
