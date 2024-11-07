using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    public float currentHP;
    private int Exp;
    private int moneyCount;
    public Slider hpSlider;     //HPバー（スライダー）
    void Start()
    {
        currentHP = HP;
        hpSlider.maxValue=HP;
        hpSlider.value = (float)currentHP;//HPバーの最初の値（最大HP）を設定
        
    }
    void Awake()
    {

    }
    // ダメージを受け取るメソッド
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        // Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);
        hpSlider.value = (float)HP / (float)currentHP;//スライダは０〜1.0で表現するため最大HPで割って少数点数字に変換
        if (gameObject.tag == "Enemy" && currentHP <= 0)
        {
            Die();
        }
    }
    public void setExp(int exp)
    {
        Exp = exp;

    }
    public float getHP()
    {
        return HP;
    }
    public void setHP(float hp)
    {
        HP = hp;
        return;
    }
    public float getCurrentHP()
    {
        return currentHP;
    }
    public void setCurrentHP(float set)
    {
        currentHP = set;
    }
    public void addHP(float hp)
    {
        HP += hp;
        return;
    }
    public void AddCurrentHP(float set)
    {
        currentHP += set;
    }
    private Vector3 CreateExpPos()
    {
        Vector3 ret;
        ret = new Vector3(0, 0, 0);
        float randomPos;//乱数ベクトル作るための一時的なもの
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
               Resources.Load<GameObject>("Exp"), CreateExpPos(), Quaternion.identity);
        }

        for (int i = 0; i < moneyCount; i++)
        {

            GameObject moneyObj = Instantiate(
               Resources.Load<GameObject>("money"), CreateExpPos(), Quaternion.identity);
        }
        Debug.Log(gameObject.name + " died.");
        // ここに死亡時の処理を書く
        Destroy(gameObject);

    }
}