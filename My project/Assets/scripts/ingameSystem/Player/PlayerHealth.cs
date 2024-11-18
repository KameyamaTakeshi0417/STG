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
        currentHP -= damage;
        OnPlayerHPChanged?.Invoke();
        if (hpSlider != null)
        {
            //SliderUpdate();
        }
        // Debug.Log(gameObject.name + " took " + damage + " damage. Remaining HP: " + currentHP);
        if (currentHP <= 0)
        {
            if (hpSlider != null)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        //ゲームオーバー処理を入れる
    }
}
