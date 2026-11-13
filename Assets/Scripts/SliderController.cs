using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    private void updateTextOnSliderChanged(TextMeshProUGUI text, float value, string textString)
    {
        text.text = value.ToString() + textString;
    }

    public void fillSliderBasedOnValue(Slider slider, float value)
    {
        slider.value = value;
    }
}
