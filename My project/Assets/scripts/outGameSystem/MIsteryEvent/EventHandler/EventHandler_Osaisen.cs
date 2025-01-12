using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler_Osaisen : _EventHandlerBase
{
    // Start is called before the first frame update
    public override void Init()
    {
        useText[0] = "お賽銭箱がある・・・・。\nお賽銭をいれたらご利益があるかもしれない。";
        useText[1] = "滅茶苦茶ご利益があった！";
        useText[2] = "ぼちぼちのご利益があった・・・。";
        useText[3] = "なーんにも起きなかった・・・！";
        base.Init();
    }

    public override void Action1()
    { //200Exp消費するイベント。一番成功率高い
        createText(CreateValue(70, 25, 5));

        changeActivateButton(4, false);
        quitButton.SetActive(true);
    }

    public override void Action2()
    { //100Exp消費。
        createText(CreateValue(40, 50, 10));
        changeActivateButton(4, false);
        quitButton.SetActive(true);
    }

    public override void Action3()
    { //50P消費
        createText(CreateValue(20, 65, 15));
        changeActivateButton(4, false);
        quitButton.SetActive(true);
    }

    public override void Action4()
    { //0P消費
        createText(CreateValue(5, 20, 75));
        changeActivateButton(4, false);
        quitButton.SetActive(true);
    }

    private void createText(int resultNum)
    {
        switch (resultNum)
        {
            case 0:
                textZone.text = useText[1];
                BigHit();
                break;
            case 1:
                textZone.text = useText[2];
                MiddleHit();
                break;

            case 2:
                textZone.text = useText[3];
                break;

            default:
                break;
        }
    }

    private int CreateValue(int value1, int value2, int value3)
    {
        //50Exp消費
        // 重み付きリストを作成。
        Dictionary<int, int> weightedNumbers = new Dictionary<int, int>()
        { //弾丸85%、レリック15%
            { 0, value1 }, // 大当たり
            { 1, value2 }, //辺り
            { 2, value3 }, //外れ、スカ
        };
        // 累積重みを計算
        int totalWeight = 0;
        foreach (var weight in weightedNumbers.Values)
        {
            totalWeight += weight;
        }
        // Randomクラスのインスタンスを作成
        System.Random random = new System.Random();

        // 配列をループして重み付けによるランダムな値を設定
        // 0から累積重みの範囲内で乱数を取得
        int randomValue = random.Next(0, totalWeight);
        int ret = 0;
        // 重みをもとに数値を選択
        foreach (var kvp in weightedNumbers)
        {
            if (randomValue < kvp.Value)
            {
                ret = kvp.Key;
                break;
            }
            randomValue -= kvp.Value;
        }
        return ret;
    }

    private void BigHit()
    {
        GameObject player = GameObject.Find("Player");
        Player playerStatusScript = player.GetComponent<Player>();
        PlayerHealth playerHPScript = player.GetComponent<PlayerHealth>();

        playerStatusScript.DamageAdd += 20;
        playerStatusScript.BlockDmg += 15;
        playerHPScript.HP += 40;
        playerHPScript.currentHP += 20;
    }

    private void MiddleHit()
    {
        GameObject player = GameObject.Find("Player");
        Player playerStatusScript = player.GetComponent<Player>();
        PlayerHealth playerHPScript = player.GetComponent<PlayerHealth>();

        playerStatusScript.DamageAdd += 5;
        playerStatusScript.BlockDmg += 5;
        playerHPScript.HP += 10;
        playerHPScript.currentHP += 5;
    }

    private void NoHit()
    {
        return;
    }
}
