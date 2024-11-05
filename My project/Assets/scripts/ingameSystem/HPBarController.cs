using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetHealth(float health)
    {
        slider.value = health;

        // Gradientを使用してバーの色を設定
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
