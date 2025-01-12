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
    {
        // 重み付きリストを作成。最上位のシークレットレアは特殊な方法でないと排出しないことにした。ギャラクシーなんちゃらの影響。
        Dictionary<int, int> weightedNumbers = new Dictionary<int, int>()
        { //弾丸85%、レリック15%
            { 0, 70 }, // 大当たり
            { 1, 25 }, //辺り
            { 2, 5 }, //外れ、スカ
        };
        // 累積重みを計算
        int totalWeight = 0;
        foreach (var weight in weightedNumbers.Values)
        {
            totalWeight += weight;
        }
        // Randomクラスのインスタンスを作成
        System.Random random = new System.Random();
        int resultNum = 0;
        // 配列をループして重み付けによるランダムな値を設定
        // 0から累積重みの範囲内で乱数を取得
        int randomValue = random.Next(0, totalWeight);

        // 重みをもとに数値を選択
        foreach (var kvp in weightedNumbers)
        {
            if (randomValue < kvp.Value)
            {
                resultNum = kvp.Key;
                break;
            }
            randomValue -= kvp.Value;
        }
        textZone.text = useText[3];
        switch (resultNum)
        {
            case 0:
                textZone.text = useText[1];
                break;
            case 1:
                textZone.text = useText[2];
                break;

            case 2:
                textZone.text = useText[4];
                break;

            default:
                break;
        }
        changeActivateButton(4, false);
        quitButton.SetActive(true);
    }

    public override void Action2() { }

    public override void Action3() { }

    public override void Action4() { }

    public override void Action5() { }
}
