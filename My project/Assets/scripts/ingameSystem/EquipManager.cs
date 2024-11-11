using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    // Start is called before the first frame update
   public GameObject activeBullet; // 実際にプレイヤーに反映されるBullet
    public GameObject stockBullet; // ストックとして保持するBullet

    public GameObject activeCase; // 実際にプレイヤーに反映されるCase
    public GameObject stockCase; // ストックとして保持するCase

    public GameObject activePrimer; // 実際にプレイヤーに反映されるPrimer
    public GameObject stockPrimer; // ストックとして保持するPrimer

    public float totalPower; // 装備による加算後のパワー
    public float totalSpeed; // 装備による加算後のスピード
    public int totalRarelity; // 装備の総レアリティ

    void Start()
    {
        CalculateStats(); // 装備状態をもとにステータスを計算
    }

    public void EquipItem(GameObject item, string type)
    {
        switch (type)
        {
            case "Bullet":
                if (item.GetComponent<Bullet_Base>() != null)
                {
                    stockBullet = activeBullet;
                    activeBullet = item;
                }
                break;
            case "Case":
                if (item.GetComponent<Case_Base>() != null)
                {
                    stockCase = activeCase;
                    activeCase = item;
                }
                break;
            case "Primer":
                if (item.GetComponent<Primer_Base>() != null)
                {
                    stockPrimer = activePrimer;
                    activePrimer = item;
                }
                break;
            default:
                Debug.LogWarning("Invalid item type");
                break;
        }
        
        CalculateStats(); // 新しい装備に応じてステータスを再計算
    }

    private void CalculateStats()
    {
        // 初期化
        totalPower = 0;
        totalSpeed = 0;
        totalRarelity = 0;

        // アクティブなBullet, Case, Primerのステータスを加算
        if (activeBullet != null)
        {
            Bullet_Base bulletScript = activeBullet.GetComponent<Bullet_Base>();
            if (bulletScript != null)
            {
                totalPower += bulletScript.getDmg();
                totalSpeed += bulletScript.getSpeed();
                totalRarelity += bulletScript.getRarelity();
            }
        }

        if (activeCase != null)
        {
            Case_Base caseScript = activeCase.GetComponent<Case_Base>();
            if (caseScript != null)
            {
                totalPower += caseScript.getDmg();
                totalSpeed += caseScript.getSpeed();
                totalRarelity += caseScript.getRarelity();
            }
        }

        if (activePrimer != null)
        {
            Primer_Base primerScript = activePrimer.GetComponent<Primer_Base>();
            if (primerScript != null)
            {
                totalPower += primerScript.getDmg();
                totalSpeed += primerScript.getSpeed();
                totalRarelity += primerScript.getRarelity();
            }
        }

        Debug.Log("Total Power: " + totalPower);
        Debug.Log("Total Speed: " + totalSpeed);
        Debug.Log("Total Rarelity: " + totalRarelity);
    }
}
