using UnityEngine;

public class Health : MonoBehaviour
{
    public float HP = 100f; // 初期HP

    // ダメージを受け取るメソッド
    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + HP);

        if (HP <= 0)
        {
            Die();
        }
    }
public float getHP(){
    return HP;
}
public void addHP(float hp){
    HP+=hp;
    return ;
}
public void setHP(float hp){
    HP=hp;
    return ;
}
    // HPが0になった時の処理
    void Die()
    {
        Debug.Log(gameObject.name + " died.");
        // ここに死亡時の処理を書く
        Destroy(gameObject);
    }
}