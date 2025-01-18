using UnityEngine;
using UnityEngine.EventSystems; // PointerEventDataを使うために必要
using UnityEngine.UI;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public GameObject parentObj;
    private rewardUIManager targetScript;

    [TextArea]
    public string NameText;

    [TextArea]
    public string ExplainText;

    void Start()
    {
        button = GetComponent<Button>();
    }

    public void Init(GameObject targetObj, GameObject rewardObj)
    {
        parentObj = targetObj;
        targetScript = parentObj.GetComponent<rewardUIManager>();
        ItemPickUp textScript = rewardObj.GetComponent<ItemPickUp>();
        NameText = textScript.nameText;
        ExplainText = textScript.explainText;
    }

    // マウスカーソルがボタンに入ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ボタンの色を変えるなどの処理をここに追加できます
        targetScript.ChangeExplainText(NameText, ExplainText);
        Debug.Log("ボタンにマウスが入った");
    }

    // マウスカーソルがボタンから出たとき
    public void OnPointerExit(PointerEventData eventData)
    {
        // ボタンの色を元に戻すなどの処理をここに追加できます
        Debug.Log("ボタンからマウスが離れた");
    }
}
