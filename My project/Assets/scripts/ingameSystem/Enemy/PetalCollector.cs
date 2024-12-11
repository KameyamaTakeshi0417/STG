using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalCollector : MonoBehaviour
{
    public Vector3 rotate; //プレイヤーに向かった時の角度
    public float reachForward; //遠いかどうか
    public float reachBehind; //近いかどうか
    public float moveMag;
    private int actionCount = 0; //
    protected EliteHealth myHealth;

    // Start is called before the first frame update
    void Awake()
    {
        myHealth = gameObject.GetComponent<EliteHealth>();
        gameObject.GetComponent<EliteHealth>().setSlideHPBar();
        StartCoroutine("Idle");
    }

    // Update is called once per frame
    void Update() { }

    private Vector3 checkDirectionToPlayer()
    {
        Vector3 ret;
        ret = GameObject.Find("Player").transform.position - gameObject.transform.position;
        return ret;
    }

    private void checkAngleToPlayer() { }

    private IEnumerator Idle()
    {
        if (myHealth.getCurrentHP() <= (myHealth.getHP() * 0.5f))
        {
            yield return attack1();
        }
        yield return new WaitForSeconds(0.5f);
        yield return moveStartPoint();
    }

    private IEnumerator moveStartPoint()
    {
        float playerLength = checkDirectionToPlayer().magnitude;
        if (playerLength >= reachForward)
        {
            yield return moveForward();
        }
        else if (playerLength <= reachBehind)
        {
            yield return movebehind();
        }
        else if (playerLength < reachForward && playerLength > reachBehind)
        {
            yield return moveleft(); //壁を認識できるものが作れたらここに逆向きを追加する
        }
        yield return null;
    }

    private IEnumerator moveForward()
    {
        Debug.Log(gameObject.name + ":forward");
        int limitClock = 1000; //待機カウント
        int count = 0;
        while (true)
        {
            count++;
            gameObject.transform.position +=
                (Vector3.Normalize(checkDirectionToPlayer())) * moveMag;

            if (checkDirectionToPlayer().magnitude < reachForward)
            {
                yield return attack0();
            }
            if (count >= limitClock)
            {
                yield return attack1();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator movebehind()
    {
        Debug.Log(gameObject.name + ":behind");
        int limitClock = 500; //待機カウント
        int count = 0;
        while (true)
        {
            count++;
            gameObject.transform.position -=
                (Vector3.Normalize(checkDirectionToPlayer())) * moveMag;

            if (checkDirectionToPlayer().magnitude >= reachForward)
            {
                yield return Idle();
            }
            if (count >= limitClock)
            {
                yield return attack0();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator moveleft()
    {
        yield return Idle();
    }

    private IEnumerator moveright()
    {
        yield return Idle();
    }

    private IEnumerator attack0Pattern(Vector3 setPosition, float waitTime)
    {
        int BulletCount = 3;
        int[] shootSet = new int[BulletCount];
        shootSet[0] = 6;
        shootSet[1] = 2;
        shootSet[2] = 10;
        float passSec = waitTime * BulletCount;
        float positionMag = 3.0f;
        for (int i = 0; i < BulletCount; i++)
        {
            Vector3 createPosition =
                gameObject.transform.position
                + setPosition * positionMag
                + (getShootWayAsClock(shootSet[i]) * positionMag * 0.4f);
            GameObject bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                createPosition,
                Quaternion.LookRotation(
                    Vector3.forward,
                    createPosition - (gameObject.transform.position + setPosition * positionMag)
                )
            );
            bullet.GetComponent<petalBullet>().wait(passSec);
            passSec -= waitTime;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator attack0()
    { //牽制技
        int[] shootSet = new int[3];
        shootSet[0] = 6;
        shootSet[1] = 2;
        shootSet[2] = 10;
        float waitTime = 1f;
        yield return attack0Pattern(getShootWayAsClock(shootSet[0]), waitTime);
        yield return new WaitForSeconds(waitTime);
        yield return attack0Pattern(getShootWayAsClock(shootSet[1]), waitTime);
        yield return new WaitForSeconds(waitTime);
        yield return attack0Pattern(getShootWayAsClock(shootSet[2]), waitTime);
        yield return new WaitForSeconds(waitTime);
        yield return Idle();
    }

    private IEnumerator ShootPattern(int startClock, bool isClockwise, int count)
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

    private IEnumerator attack1()
    {
        yield return ShootPattern(2, true, 50);
        yield return ShootPattern(10, false, 50);
        yield return ShootPattern(6, true, 50);
        yield return Idle();
    }

    private IEnumerator attack2()
    { //通常攻撃強
        yield return null;
    }

    private IEnumerator attack3()
    { //強攻撃
        yield return null;
    }

    private IEnumerator attack4()
    { //奥義
        yield return null;
    }

    public Vector3 getShootWayAsClock(int num)
    {
        Vector3 ret = new Vector3(0, 0, 0);
        switch (num)
        {
            case 0:
            case 12:
                ret = new Vector3(0, 1, 0);
                break;
            case 1:
                ret = new Vector3(0.5f, 0.866f, 0);
                break;
            case 2:
                ret = new Vector3(0.866f, 0.5f, 0);
                break;
            case 3:
                ret = new Vector3(1, 0, 0);
                break;
            case 4:
                ret = new Vector3(0.866f, -0.5f, 0);
                break;
            case 5:
                ret = new Vector3(0.5f, -0.866f, 0);
                break;
            case 6:
                ret = new Vector3(0, -1, 0);
                break;
            case 7:
                ret = new Vector3(-0.5f, -0.866f, 0);
                break;
            case 8:
                ret = new Vector3(-0.866f, -0.5f, 0);
                break;
            case 9:
                ret = new Vector3(-1, 0, 0);
                break;
            case 10:
                ret = new Vector3(-0.866f, 0.5f, 0);
                break;
            case 11:
                ret = new Vector3(-0.5f, 0.866f, 0);
                break;
            default:
                ret = new Vector3(0, -1, 0);
                break;
        }
        return ret;
    }
}
