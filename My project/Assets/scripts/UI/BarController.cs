using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public int min = 0;
    public int max ;
    public int currentValue = 0;

    public Image barImage;
    public Text valueText;

    public Button increaseButton;
    public Button decreaseButton;

    private int maxBarUnits=100 ; // バーの目盛りの最大単位数

    void Start()
    {
        maxBarUnits=max;
        // ボタンのクリックイベントにリスナーを追加
        increaseButton.onClick.AddListener(IncreaseValue);
        decreaseButton.onClick.AddListener(DecreaseValue);

        // 初期値の設定
        UpdateBar();
    }

    void IncreaseValue()
    {
        if (currentValue < max)
        {
            currentValue += 10; // 10単位で増加
            UpdateBar();
        }
    }

    void DecreaseValue()
    {
        if (currentValue > min)
        {
            currentValue -= 10; // 10単位で減少
            UpdateBar();
        }
    }

    void UpdateBar()
    {
        maxBarUnits=max;
        // バーの長さを更新
        float fillAmount = (float)currentValue / maxBarUnits;
        barImage.fillAmount = fillAmount;

        // テキストを更新
        valueText.text = currentValue.ToString();
    }
}
