using UnityEngine;

public class Health : MonoBehaviour
{
    private float HP = 100f; // 初期HP
private int Exp;

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
        public void setExp(int exp){
             Exp=exp;

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
      private Vector3 CreateExpPos()
    {
        Vector3 ret;
        ret=new Vector3(0,0,0);
        float randomPos;//乱数ベクトル作るための一時的なもの
        randomPos = Random.Range(-2f, 2f);
        ret.x = randomPos;
        randomPos = Random.Range(-2f, 2f);
        ret.y=randomPos;
ret+=transform.localPosition;
        return ret;
    }
    // HPが0になった時の処理
    void Die()
    {

        for(int i=0;i<Exp;i++){
       
         GameObject ExpObj = Instantiate(
            Resources.Load<GameObject>("Exp"), CreateExpPos(), Quaternion.identity);
        }
         Debug.Log(gameObject.name + " died.");
               // ここに死亡時の処理を書く
        Destroy(gameObject);

    }
}