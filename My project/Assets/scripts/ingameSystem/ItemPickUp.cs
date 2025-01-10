using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemData itemData; // アイテムの情報を持つ ScriptableObject
    public string itemType; // アイテムの種類（Bullet, Case, Primer）
    public int itemRarelity;
    public GameObject targetObj;
    public static ItemPickUp Instance { get; private set; }
    public string accessAddress;

    private static List<GameObject> persistentObjects = new List<GameObject>();

    void Start()
    {
        // このオブジェクトがすでにDontDestroyOnLoadのリストに含まれていない場合のみ追加
        if (!persistentObjects.Contains(gameObject))
        {
            persistentObjects.Add(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 同じオブジェクトが存在する場合、破棄する
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // プレイヤーに触れた場合、インベントリに追加し装備を更新
            EquipManager equipManager = GameObject.Find("GameManager").GetComponent<EquipManager>();
            InventoryManager inventoryManager = GameObject
                .Find("GameManager")
                .GetComponent<InventoryManager>();

            if (equipManager != null)
            {
                GameObject targetObj = Resources.Load<GameObject>(accessAddress);
                equipManager.EquipItem(targetObj);

                inventoryManager.AddAmmo(targetObj);

                Debug.Log("Picked up: " + itemData.itemName);
                Destroy(gameObject);
            }

            // 自身を破壊
        }
    }

    public ItemPickUp(int rarelity)
    {
        itemRarelity = rarelity;
    }

    public void setSpecialEffect()
    {
        //ここにレリックとかのプレイヤーの補正値を記載する・・・のかな]
        //例えば基本1.0の補正値を用意して、その値に影響を与えるって感じ。
        //プレイヤー側に用意して、ダメージ計算式を作る必要があるな。
        //敵に与えられるダメージ=（(bulletDamage+playerPow)*補正値）＋特別な補正値
        //か？特別な補正値がキモで、例えば弾速が100倍になる代わりに単発威力が1%になるレリックを用意したとして、
        //プレイヤー側はこの特別補正値を作ってくれるレリックを装備すればデメリットを踏み倒せる。
        //説明が必要になるな。
        //攻撃力、威力、追加ダメージって説明にする？攻撃力はプレイヤーの数値、威力は弾本来のダメージって伝えればこれは問題ない。
        //「攻撃に追加でx点の固定ダメージを付与する。」固定ダメージ。これだな。
        //固定ダメージのレリックは取得で弾速度を10%早めるだけで、装備したら追加で30%増加与ダメージ50%減、固定ダメージ5点付与って感じにする？
        //（攻撃力、威力の合計が与ダメージ。pow10dmg10=与ダメ20。）
        //オッケー。これで。
        //じゃあスピード系の石の意志について。
        //取得したら最大HP80増回復なし。装備したら通常移動が70%遅くなるが、ダメージを減らす。。。ダメージを減らす処理ウザすぎる。絶対実装したくない。やめてくれ
        //いや、そうでもないか？
        //プレイヤーのヘルスに補正値をつけて、takeDmgの処理に書きか加えるだけでよいかもしれん。
    }
}
