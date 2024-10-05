using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField]
    private GameObject DamageObj;
    [SerializeField]
    private GameObject PosObj;
    [SerializeField]
    private Vector3 AdjPos;
    private float HP = 100f; // 初期HP

    public float currentHP;
private int Exp;
void Start(){
    currentHP=HP;
}
    // ダメージを受け取るメソッド
    public void TakeDamage(float damage)
    {        
        ViewDamage(damage);
        currentHP -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);

        if (gameObject.tag=="Enemy"&&currentHP <= 0)
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
public void setHP(float hp){
    HP=hp;
    return ;
}
public float getCurrentHP(){
    return currentHP;
}
public void setCurrentHP(float set){
    currentHP=set;
}
public void addHP(float hp){
    HP+=hp;
    return ;
}
public void AddCurrentHP(float set){
    currentHP+=set;
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
    private void ViewDamage(float _damage)
    {
        GameObject _damageObj = Instantiate(DamageObj);
        _damageObj.GetComponent<TextMesh>().text = _damage.ToString();
        _damageObj.transform.position = PosObj.transform.position + AdjPos;
    }
}