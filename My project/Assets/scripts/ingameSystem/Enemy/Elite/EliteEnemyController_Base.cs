using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteEnemyController_Base : MonoBehaviour
{
    // Start is called before the first frame update    public Vector3 rotate; //プレイヤーに向かった時の角度
    public float reachForward; //遠いかどうか
    public float reachBehind; //近いかどうか
    public float moveMag;

    protected EliteHealth myHealth;
    public bool isLifeBroken = false;
    public int Level = 1; //行動を変更する指標であり倒したら貰える花弁の最低枚数

    // Start is called before the first frame update
    void Awake()
    {
        myHealth = gameObject.GetComponent<EliteHealth>();
        gameObject.GetComponent<EliteHealth>().setSlideHPBar();
        StartCoroutine("Idle");
    }

    // Update is called once per frame
    void Update() { }

    private IEnumerator Idle()
    {
        //レベルを条件に追加したいね
        if (myHealth.LifeCount == 2 && myHealth.getCurrentHP() <= (myHealth.getHP() * 0.5f))
        {
            //    yield return attack1();
        }
        if (myHealth.LifeCount == 1)
        {
            //     yield return attack2();
        }
        yield return new WaitForSeconds(0.5f);
        yield return moveStartPoint();
    }

    protected Vector3 checkDirectionToPlayer()
    {
        Vector3 ret;
        ret = GameObject.Find("Player").transform.position - gameObject.transform.position;
        return ret;
    }

    protected IEnumerator moveStartPoint()
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

    protected IEnumerator moveForward()
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
                //  yield return attack0();
            }
            if (count >= limitClock)
            {
                //  yield return attack1();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    protected IEnumerator movebehind()
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
                // yield return attack0();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    protected IEnumerator moveleft()
    {
        yield return Idle();
    }

    protected IEnumerator moveright()
    {
        yield return Idle();
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
