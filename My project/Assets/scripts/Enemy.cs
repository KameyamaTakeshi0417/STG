using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class enemy : MonoBehaviour
{
    public Vector3 shamblingWay;//徘徊時のルート
    public Vector3 playerPos;//プレイヤーの位置
    public Vector3 enemyIniPos;//エネミーの初期位置
    public Vector3 enemyPos;//エネミーの現在位置
    public float sight;//プレイヤーを認識する範囲
    public float enemyRange;//エネミーの移動する範囲
    public float duration = 3.0f;//移動時間
    public float elapsedTime = 0.0f;//経過時間

    // Start is called before the first frame update
    void Start()
    {
        enemyIniPos = transform.position;
        
        shamblingWayUpdate();

        elapsedTime = 0.0f;

        StartCoroutine(shambling(shamblingWay,duration));
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = GameObject.Find("Player").transform.position;//プレイヤーの現在位置
        enemyPos  = transform.position ;//エネミーの現在位置
    }

    //shamblingWayの更新
    void shamblingWayUpdate()
    {
        float randomPos;//乱数ベクトル作るための一時的なもの

        randomPos = UnityEngine.Random.Range(-1f, 1f);
        shamblingWay.x = randomPos;
        randomPos = UnityEngine.Random.Range(-1f, 1f);
        shamblingWay.y=randomPos;

        return;
    }

    //エネミーの行動決定
    private IEnumerator shambling(Vector3 goalPos,float durationTime)
    {
        float travelTime;

        Vector3 startPos=transform.position;
        Vector3 endPos = goalPos;

        //shamblingWayの更新
        shamblingWayUpdate();
        elapsedTime = 0.0f;
        
        while(true)
        {

            // 経過時間を更新
            elapsedTime += Time.deltaTime;
            travelTime = elapsedTime / durationTime;
            //transform.position = Vector3.Lerp(startPos, endPos, t);
            //敵の移動範囲の確認
            enemyRangeDec();
            //Enemyの移動
            transform.Translate( shamblingWay * Time.deltaTime );
            // 移動が完了したかチェック
            if (travelTime >= 1.0f)
            {
                break;//whileから抜け出す
               
            }

            //ここに攻撃ルーチン開始を入れる
            if (getRangeToPlayer() <= sight)
            {
               yield return LockOn(getWayToPlayer());
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

    //プレイヤー確認時の行動
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
        if (getRangeToPlayer() < sight)
        {
            yield return shoot();
        }
        else
        {
            //範囲外に出たらshambling再開
            yield return shambling(shamblingWay, duration);
        }

        yield return null;
    }

    //プレイヤーを撃つ
    private IEnumerator shoot() {
        Destroy(gameObject);
        //弾発射。3回くらい。都度10fのロックオン(コルーチン呼び出しでなく、ここで同様の処理を記載)、
        //射撃制度を落としたいので5fくらい待機、発射。プレイヤーの位置を問わず、3回繰り返す。
        //3発打ち終わったら10f待つ。
        //範囲内にプレイヤーがいたらshootに戻る。
        //範囲外に出たらshambling再開。
        yield return shambling(shamblingWay, duration); //プレイヤーがいなかったら再帰的にshambling呼び出し。

    }

    //プレイヤーの位置情報取得
    private Vector3 getWayToPlayer() {
        Vector3 ret;

        ret= enemyPos - playerPos;

        return ret;
    }
    
    //プレイヤーとの距離を算出
    private float getRangeToPlayer() {
        float ret=0.0f;
        Vector3 length;
        //プレイヤーとの距離をベクトルで作って、範囲内ならret返す。そうでないならマイナス返す。
        length = getWayToPlayer();
        ret=length.magnitude;
        ret=Mathf.Abs(ret);//マイナスいらない
        if(ret<=sight)return ret;
        return sight + 1.0f;
    }

    //エネミーの行動範囲の決定
    void enemyRangeDec(){
        Vector3 vec;//ベクトル
        float length;//長さ
        //エネミーの初期位置と現在位置の差
        vec = enemyPos - enemyIniPos;
        //ベクトルを長さに変換
        length = vec.magnitude;
        //絶対値
        length = Mathf.Abs(length);
        //スポーンから行動範囲を超えた場合、移動経路を反転する
        if(length >= enemyRange){
            shamblingWay.x *= -1.0f;
            shamblingWay.y *= -1.0f;
        }

        return;
    }
}
