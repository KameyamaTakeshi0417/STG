using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HimawariController : EliteEnemyController_Base
{
    private int actionCount = 0; //
    public GameObject arrowPrefab; // 矢印のプレハブ
    public float moveTime = 0.35f; // 移動にかかる時間（秒）
    public float arrowAlpha = 0.5f; // 矢印の透明度
    private GameObject arrow; // 表示する矢印オブジェクト

    // Start is called before the first frame update
    void Awake()
    {
        myHealth = gameObject.GetComponent<EliteHealth>();
        gameObject.GetComponent<EliteHealth>().setSlideHPBar();
        StartCoroutine("Idle");
    }

    // Update is called once per frame
    void Update() { }

    private void checkAngleToPlayer() { }

    private IEnumerator Idle()
    {
        //レベルを条件に追加したいね
        if (myHealth.LifeCount >= 3)
        {
            yield return attack1();
        }
        if (myHealth.LifeCount == 2)
        {
            yield return attack2();
        }
        if (myHealth.LifeCount == 1)
        {
            yield return attack3();
        }
        yield return new WaitForSecondsRealtime(0.1f);
        yield return moveStartPoint();
    }

    private IEnumerator attack1()
    {
        int createCount = 12;

        //プレイヤーの近くに移動した後、ヒマワリ型に弾丸を生成する。
        //生成時、ヒマワリの周辺に追加で雌蕊形のバリアを展開する。
        //可能なら移動方向を事前表示して、その後急速に移動するって処理を入れたい


        //まずはプレイヤーに向かって移動
        GameObject playerObj = GameObject.Find("Player");
        Vector3 targetPosition = playerObj.transform.position;
        Vector3 direction = targetPosition - transform.position; // プレイヤーに向かうベクトル
        float distance = direction.magnitude; // ボスとプレイヤーの距離

        // 2〜3秒の間に矢印を生成し続ける
        float timeElapsed = 0f;
        while (timeElapsed < 2f) // 2秒間矢印を作り続ける
        {
            timeElapsed += Time.deltaTime;

            // 矢印を生成する
            CreateArrow(direction, distance);
            yield return null; // 1フレーム待機
        }

        // nフレームの間にプレイヤーに向かって移動する
        float moveSpeed = distance / moveTime; // 移動速度
        float timeToMove = 0f;

        while (timeToMove < moveTime)
        {
            timeToMove += Time.deltaTime;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null; // 1フレーム待機
        }

        // 最終的にプレイヤーの位置に到達
        transform.position = targetPosition;
        Destroy(arrow); // 矢印を削除

        //雌蕊バリヤ生成処理を入れる

        //弾丸の展開
        int refrainCountMax = 3;
        int refrainCount = 0;
        while (refrainCount < refrainCountMax)
        {
            for (int i = 0; i < createCount; i++)
            {
                attack1Pattern(i); //12個生成
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(0.02f);
            refrainCount++;
        }

        yield return new WaitForSecondsRealtime(0.15f);
        yield return Idle();
    }

    private void attack1Pattern(int createIndex)
    {
        Vector3 moveWay = base.getShootWayAsClock(createIndex);
        moveWay.Normalize();
        float rotationAngle = Mathf.Atan2(moveWay.y, moveWay.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
            Resources.Load<GameObject>("Objects/Bullet/PetalBullet_Himawari"),
            gameObject.transform.position,
            Quaternion.Euler(new Vector3(0, 0, rotationAngle + 90))
        );
        bullet.GetComponent<PetalBullet_Himawari>().MoveStraignt(createIndex);
    }

    private void CreateArrow(Vector3 direction, float distance)
    {
        // 矢印オブジェクトがすでに存在している場合は削除
        if (arrow != null)
        {
            Destroy(arrow);
        }

        // 矢印を生成
        arrow = Instantiate(arrowPrefab, transform.position, Quaternion.LookRotation(direction));

        // 矢印のスケールを調整（長さをボスからプレイヤーまでの距離に設定）
        arrow.transform.localScale = new Vector3(
            arrow.transform.localScale.x,
            arrow.transform.localScale.y,
            distance
        );

        // 矢印のアルファ値を設定（透明度）
        Renderer arrowRenderer = arrow.GetComponent<Renderer>();
        if (arrowRenderer != null)
        {
            Material mat = arrowRenderer.material;
            Color color = mat.color;
            color.a = arrowAlpha;
            mat.color = color;
        }
    }

    private IEnumerator attack2()
    { //通常攻撃強
        yield return attack2Pattern(2, true, 50);
        yield return attack2Pattern(10, false, 50);
        yield return attack2Pattern(6, true, 50);
        yield return Idle();
    }

    private IEnumerator attack2Pattern(int startClock, bool isClockwise, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet.GetComponent<petalBullet>().ShootInvolute(startClock, isClockwise);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator attack3()
    { //強攻撃
        /*
        予定としてはattack2の挙動をインボリュート後の動きをホーミングから中心に向かって直進+バラまき+ホーミングでいこうかな。
        */
        yield return null;
    }

    private IEnumerator attack4()
    { //奥義
        yield return null;
    }
}
