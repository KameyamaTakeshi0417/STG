using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GoodsHandler : MonoBehaviour
{
    public TextMeshProUGUI priceText;
    public Image targetImage;
    public GameObject setGoods; //実装確認でき次第public外す

    public void Init(GameObject goods)
    {
        targetImage = this.gameObject.GetComponent<Image>();
        setGoods = goods;
    } //初期化はここで設定し、MarchantManagerで呼び出す

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void Purchase() { }
}
