using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider1, healthSlider2;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        displayHealthUponStart(healthSlider1, gameManager.player1Stats.health);
        displayHealthUponStart(healthSlider2, gameManager.player2Stats.health);
    }


    public void displayHealthUponStart(Slider slider, float health)
    {
        setSliderMaxValue(slider, health);
        fillSliderBasedOnValue(slider, health);
    }

    private void setSliderMaxValue(Slider slider, float maxValue)
    {
        slider.maxValue = maxValue;
    }

    private void fillSliderBasedOnValue(Slider slider, float value)
    {
        slider.value = value;
    }
}
