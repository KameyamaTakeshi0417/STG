using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float addPow;
    public float addShootSpeed;
    public float addHP;
    public string itemName;
    public int rarelity;
    public ItemData itemData;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            InItemBag();
        }
    }

    private void InItemBag()
    {
        GameObject manager = GameObject.Find("GameManager");
        manager.GetComponent<ItemManager>().AddItemData(itemData);
        Destroy(this.gameObject);
    }

    public Item(string name, int value)//コンストラクタ。新しく生成されるときに引数として使えるやつ。すっかり忘れてたわ
    {
        itemName = name;
        rarelity = value;
    }
}
