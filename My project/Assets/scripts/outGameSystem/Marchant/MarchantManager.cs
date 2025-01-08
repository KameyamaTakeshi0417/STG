using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchantManager : MonoBehaviour
{
    public GameObject targetCanvas;
    public GameObject AmmoMergeButton;
    int AmmoGoodsNum = 5;
    public GameObject[] AmmoGoods = new GameObject[5];

    int RelicNum = 3;
    public GameObject[] RelicGoods = new GameObject[3];

    // Start is called before the first frame update
    void Start()
    {
        GoodsHandler targetObjScript = null;
        for (int i = 0; i < AmmoGoodsNum; i++)
        {
            targetObjScript = AmmoGoods[i].GetComponent<GoodsHandler>();
            if (targetObjScript != null)
            {
                //targetObjScript.Init()
            }
        }
    }

    // Update is called once per frame
    void Update() { }

    public GameObject createAmmoParts()
    {
        string CreateObjPath = null;
        Random.InitState((int)(System.DateTime.Now.Ticks & 0xFFFFFFF));
        CreateObjPath =
            "Objects/Reward/"
            + rewardUIManager.AmmoTypeArray[Random.Range(0, rewardUIManager.AmmoTypeArray.Length)];
        Random.InitState((int)((System.DateTime.Now.Ticks * 31) & 0xFFFFFFF));
        CreateObjPath +=
            rewardUIManager.AmmoCategoryArray[
                Random.Range(0, rewardUIManager.AmmoCategoryArray.Length)
            ] + "_RewardObject";
        GameObject prefab = Resources.Load<GameObject>(CreateObjPath);
        return prefab;
    }
}
