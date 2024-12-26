using UnityEngine;
using UnityEngine.UI;

public class EquipButtonHandler : MonoBehaviour
{
    public string mainType; // Active または Sub
    public string itemType; // Primer, Case, または Bullet
    public GameObject parentObject;
    private equipMenuManager targetScript;

    private Button button;

    void Start()
    {
        targetScript = parentObject.GetComponent<equipMenuManager>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        Debug.Log($"Button clicked! MainType: {mainType}, ItemType: {itemType}");

        // 必要に応じて、ここで文字列を利用した処理を追加
        ProcessSelection(mainType, itemType);
    }

    private void ProcessSelection(string main, string item)
    {
        // 選択に応じた処理
        if (main == "Active" && item == "Primer")
        {
            Debug.Log("Active Primer selected!");
        }
        else if (main == "Sub" && item == "Case")
        {
            Debug.Log("Sub Case selected!");
        }
        else
        {
            Debug.Log($"Unknown combination: {main}, {item}");
        }
        targetScript.callEquipScrollBar(main, item);
    }
}
