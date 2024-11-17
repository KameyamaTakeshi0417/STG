using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar_Enemy : HPBar_Base
{
    void OnEnable()
    {
        Health.OnHPChanged += setSlideHPBar;
    }

    void OnDisable()
    {
        Health.OnHPChanged -= setSlideHPBar;
    }


    // Start is called before the first frame update
    public override void setSlideHPBar()
    {
        GameObject canvasInstance = Instantiate(
            Resources.Load<GameObject>("UI/EnemyHPCanvas"),
            gameObject.transform.position,
            Quaternion.identity
        );
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
}
