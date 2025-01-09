using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GoodsHandler : MonoBehaviour
{
    public TextMeshProUGUI priceText;
    public Image targetImage;
    public GameObject setGoods; //実装確認でき次第public外す
    public int Price;
    string imagePath = "Texture/UI/SoldOut"; // Resourcesフォルダ内の画像パス
    bool isBuy = false;

    public void Init(GameObject goods)
    {
        SpriteRenderer spriteRenderer = goods.GetComponent<SpriteRenderer>();
        targetImage = this.gameObject.GetComponent<Image>();
        targetImage.sprite = spriteRenderer.sprite;
        setGoods = goods;
    } //初期化はここで設定し、MarchantManagerで呼び出す

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void Purchase()
    {
        if (isBuy == false)
        {
            GameObject.Find("GameManager").GetComponent<InventoryManager>().AddItem(setGoods);
            targetImage.sprite = Resources.Load<Sprite>(imagePath);
            isBuy = true;
        }
    }
}
