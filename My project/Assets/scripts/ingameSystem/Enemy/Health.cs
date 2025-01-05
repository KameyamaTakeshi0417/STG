using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : _Health_Base
{
    public delegate void HPChangedHandler();
    public static event HPChangedHandler OnHPChanged;
    GameObject canvasInstance;
    Transform canvasTransform; // エネミーのCanvasのTransform
    public GameObject damageTextPrefab; // ダメージ表示用のプレハブ
    public float DamageUIMagnitude = 0.1f;

    void Start()
    {
        currentHP = HP;
        m_handler = gameObject.GetComponent<HPBar_Base>();
    }

    public virtual void setSlideHPBar()
    {
        canvasInstance = Instantiate(
            Resources.Load<GameObject>("UI/EnemyHPCanvas"),
            gameObject.transform.position,
            Quaternion.identity
        );
        canvasTransform = canvasInstance.transform;
        canvasInstance.GetComponent<HPBarFollower>().setTargetTransform(gameObject.transform);
        //canvasInstance.transform.SetParent(transform);
        canvasInstance.transform.localPosition = new Vector3(0, 2, 0); // 必要に応じてオフセットを調整
        // HPバー(Slider)を取得
        hpSlider = canvasInstance.transform.Find("HPBar").GetComponent<Slider>();

        if (hpSlider != null)
        {
            if (hpSlider != null)
            {
                // HPバーの初期設定
                hpSlider.maxValue = HP;
                hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定
            }
        }
        else
        {
            Debug.LogWarning("Canvas or HPBar not found in the enemy object.");
        }
    }

    void Awake() { }

    // ダメージを受け取るメソッド
    public override void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (hpSlider != null)
        {
            SliderUpdate();
            ShowDamage(damage);
        }
        // Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);
        if (gameObject.tag == "Enemy" && currentHP <= 0)
        {
            if (hpSlider != null)
            {
                Destroy(hpSlider);
            }

            Die();
        }
    }

    private Vector3 CreateExpPos()
    {
        Vector3 ret;
        ret = new Vector3(0, 0, 0);
        float randomPos; //乱数ベクトル作るための一時的なもの
        randomPos = Random.Range(-2f, 2f);
        ret.x = randomPos;
        randomPos = Random.Range(-2f, 2f);
        ret.y = randomPos;
        ret += transform.localPosition;
        return ret;
    }

    public void setMoneyCount(int count)
    {
        moneyCount = count;
    }

    // HPが0になった時の処理
    void Die()
    {
        for (int i = 0; i < Exp; i++)
        {
            GameObject ExpObj = Instantiate(
                Resources.Load<GameObject>("Objects/MoneyAndExp"),
                CreateExpPos(),
                Quaternion.identity
            );
        }

        Debug.Log(gameObject.name + " died.");
        // ここに死亡時の処理を書く
        Destroy(gameObject);
    }

    public void ShowDamage(float damage)
    {
        // ダメージテキストの生成
        GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
        damageTextInstance.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // テキストのスケールを調整
        damageTextInstance.GetComponent<RectTransform>().localScale =
            Vector3.one * DamageUIMagnitude;

        // テキスト内容を設定
        damageTextInstance.GetComponent<DamageUI3D>().damage = damage;
    }
}
