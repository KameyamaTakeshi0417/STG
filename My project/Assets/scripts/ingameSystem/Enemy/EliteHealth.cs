using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EliteHealth : Health
{
    public CircleGauge[] circleGauge = new CircleGauge[3];
    public int HealthPhase;
    public int LifeCount = 1;
    public GameObject[] CircleHPBar = new GameObject[3];
    public GameObject canvasInstance;

    void Start()
    {
        currentHP = HP;
        m_handler = gameObject.GetComponent<HPBar_Base>();
        for (int i = 0; i < 3; i++)
        {
            circleGauge[i] = CircleHPBar[i].GetComponent<CircleGauge>();
        }
    }

    void Awake() { }

    // ダメージを受け取るメソッド
    public override void TakeDamage(float damage)
    {
        currentHP -= damage;
        circleGauge[LifeCount - 1].setMaxHP(HP);
        circleGauge[LifeCount - 1].setCurrentHP(currentHP);
        if (hpSlider != null)
        {
            EliteSliderUpdate();
        }
        // Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);
        if (LifeCount <= 0)
        {
            Die();
        }
    }

    public void EliteSliderUpdate()
    {
        hpSlider.value = currentHP; //スライダは０〜1.0で表現するため最大HPで割って少数点数字に変換
        if (hpSlider.value <= 0)
        {
            currentHP = HP;
            hpSlider.value = currentHP;

            circleGauge[LifeCount].setMaxHP(HP);
            circleGauge[LifeCount].setCurrentHP(currentHP);
            LifeCount -= 1;
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
        Destroy(CircleHPBar[1].transform.root);
        Destroy(this.gameObject);
    }

    public override void setSlideHPBar()
    {
        canvasInstance.GetComponent<HPBarFollower>().setTargetTransform(gameObject.transform);
        canvasInstance.transform.localPosition = new Vector3(0, 2, 0); // 必要に応じてオフセットを調整

        hpSlider = canvasInstance.transform.Find("HPBar").GetComponent<Slider>();
        if (hpSlider != null)
        {
            SetCircleBar(LifeCount);
            // HPバーの初期設定
            hpSlider.maxValue = HP;
            hpSlider.value = (float)currentHP;

            // 円形ゲージの設定
            for (int i = 0; i < LifeCount; i++)
            {
                circleGauge[i] = canvasInstance.GetComponentInChildren<CircleGauge>();
                if (circleGauge != null)
                {
                    // HPバーの初期設定
                    hpSlider.maxValue = HP;
                    hpSlider.value = (float)currentHP; // HPバーの最初の値を現在のHPに設定
                    circleGauge[i].setMaxHP(HP);
                    circleGauge[i].setCurrentHP(currentHP);
                }
            }
        }
        else
        {
            Debug.LogWarning("HPBar Slider not found in the enemy object.");
        }
    }

    public void SetCircleBar(int num)
    {
        for (int i = 0; i < 3; i++)
        {
            CircleHPBar[i].SetActive(false);
            if (i < num)
            {
                CircleHPBar[i].SetActive(true);
            }
        }
    }
}
