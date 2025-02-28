using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HimawariController : EliteEnemyController_Base
{
    private int actionCount = 0; //

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
        if (myHealth.LifeCount == 2 && myHealth.getCurrentHP() <= (myHealth.getHP() * 0.5f))
        {
            yield return attack1();
        }
        if (myHealth.LifeCount == 1)
        {
            yield return attack2();
        }
        yield return new WaitForSeconds(0.5f);
        yield return moveStartPoint();
    }

    private IEnumerator attack1()
    {
        int createCount = 5;
        int[] createIndex = new int[createCount];
        createIndex[0] = 9;
        createIndex[1] = 3;
        createIndex[2] = 10;
        createIndex[3] = 2;
        createIndex[4] = 12;
        for (int i = 0; i < createCount; i++)
        {
            attack1Pattern(createIndex[i]);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        yield return Idle();
    }

    private void attack1Pattern(int createIndex)
    {
        float posMag = 7.0f;
        Vector3 moveWay =
            gameObject.transform.position
            - ((gameObject.transform.position) + base.getShootWayAsClock(createIndex) * posMag);
        moveWay.Normalize();
        float rotationAngle = Mathf.Atan2(moveWay.y, moveWay.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
            Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
            gameObject.transform.position,
            Quaternion.Euler(new Vector3(0, 0, rotationAngle + 90))
        );
        bullet
            .GetComponent<petalBullet>()
            .moveLerp(
                gameObject.transform.position,
                (gameObject.transform.position) + getShootWayAsClock(createIndex) * posMag
            );
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
