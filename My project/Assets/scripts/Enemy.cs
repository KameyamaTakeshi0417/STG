using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class enemy : MonoBehaviour
{
   public Vector3 shamblingWay;//徘徊時のルート
    public float sight;//プレイヤーを認識する範囲

    public float duration = 3.0f;//移動時間
    // 内部カウンター
    public float elapsedTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        float randomPos;//乱数ベクトル作るための一時的なもの
        randomPos= Random.Range(0.0f, 10.0f);
        shamblingWay.x = randomPos;
        randomPos = Random.Range(0.0f, 10.0f);
        shamblingWay.y=randomPos;

        shamblingWay += transform.position;
        elapsedTime = 0.0f;

        StartCoroutine(shambling(shamblingWay,duration*0.5f));//最初は片道なので移動時間半分。
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public float t;
    private IEnumerator shambling(Vector3 goalPos,float durationTime)
    {
        Vector3 startPos=transform.position;
        Vector3 endPos = goalPos;

        Vector3 playerPos;

        elapsedTime = 0.0f;
        playerPos =GameObject.Find("Player").transform.position;//プレイヤーの位置確保
        
        while(true)
        {

            // 経過時間を更新
            elapsedTime += Time.deltaTime;
            t = elapsedTime / durationTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            // 移動が完了したかチェック
            if (t >= 1.0f)
            {
                break;//whileから抜け出す
               
            }

            //ここに攻撃ルーチン開始を入れる
            if (getRangeToPlayer() >= sight)
            {
               // yield return LockOn(getWayToPlayer());
            }
            yield return new WaitForEndOfFrame();//1f待つ
        }

        int count = 0;//内部カウンター
        while (true) {
            count++;
            if (count >= 90){//90f待つ。数字は何となく。
                yield return shambling((transform.position + goalPos * (-2.0f)) , duration); //プレイヤーがいなかったら再帰的にshambling呼び出し。
            }
            yield return new WaitForEndOfFrame();
        }
       
    }
    private IEnumerator LockOn(Vector3 wayToPlayer) {
        //90fくらいプレイヤーを注視する。
        int count= 0;
        while(true) {
            count++;
            //プレイヤーを見つめる処理。角度をなんかする
            if (count >= 90) break;
        yield return new WaitForEndOfFrame();
        }

        //範囲内ならshootに移行。
        if (getRangeToPlayer() > 0)
        {
            yield return shoot();
        }
        else
        {
            //範囲外に出たらshambling再開
            yield return shambling(shamblingWay, duration);//ここキモい。
        }

        yield return null;
    }
    private IEnumerator shoot() {
        //弾発射。3回くらい。都度10fのロックオン(コルーチン呼び出しでなく、ここで同様の処理を記載)、
        //射撃制度を落としたいので5fくらい待機、発射。プレイヤーの位置を問わず、3回繰り返す。
        //3発打ち終わったら10f待つ。
        //範囲内にプレイヤーがいたらshootに戻る。
        //範囲外に出たらshambling再開。
        yield return shambling(shamblingWay, duration); //プレイヤーがいなかったら再帰的にshambling呼び出し。

    }
    private Vector3 getWayToPlayer() {
        Vector3 ret;
        ret= transform.position - GameObject.Find("Player").transform.position;
        return ret;
    }
    private float getRangeToPlayer() {
        float ret=0.0f;
        Vector3 length;
        //プレイヤーとの距離をベクトルで作って、範囲内ならret返す。そうでないならマイナス返す。
        length = getWayToPlayer();
        ret=length.magnitude;
        ret=Mathf.Abs(ret);//マイナスいらない
        if(ret>=sight)return ret;
        return -1.0f;
    }
}
