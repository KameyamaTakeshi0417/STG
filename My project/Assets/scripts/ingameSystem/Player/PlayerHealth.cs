using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : _Health_Base
{
    public delegate void PlayerHPChangedHandler();
    public static event PlayerHPChangedHandler OnPlayerHPChanged;
    public HPBar_Base myHPBarScript;
    public GameObject HPBarObj;

    // Start is called before the first frame update
    void Start()
    {
        myHPBarScript = HPBarObj.GetComponent<PlayerHPBar>();
    }

    // Update is called once per frame
    void Update() { }

    public override void TakeDamage(float damage)
    {
        float setDmg = damage;
        if (VulnerableFlg)
        {
            setDmg *= 1.5f;
        }
        currentHP -= setDmg;
        OnPlayerHPChanged?.Invoke();
        Debug.Log("TakeDamaged");
        if (hpSlider != null)
        {
            //SliderUpdate();
        }

        if (currentHP <= 0)
        {
            Debug.Log("CallDie");
            Die();
        }
    }

    public void Die()
    {
        //ゲームオーバー処理を入れる
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        Debug.Log("マネージャー見つからねえ");
        manager.GameOver();
    }
}
