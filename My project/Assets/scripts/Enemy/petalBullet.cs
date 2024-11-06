using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class petalBullet : MonoBehaviour
{
    public int cycleCount;
    public int cycleCountMax;
    public float bulletSpeedMag;
    // Start is called before the first frame update
    void Awake()
    {
        cycleCount = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void shoot(int shootWay)
    {
        StartCoroutine("spread", getShootWayAsClock(shootWay));
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

    public void ShootInvolute(int wayClock,bool rotationWay)
    {//rotWayがtrueなら反時計回り、falseなら時計回り
        Vector3 target;
        target=getShootWayAsClock(wayClock);
        Vector2 ret=new Vector2(target.x,target.y);
        float checker=1;
        if(!rotationWay)
        {
            checker=-1;
        }

        StartCoroutine(Involute(ret,checker));
    }
    private IEnumerator Involute(Vector2 startPos,float rotationWay)
    {
        float radius = 1.0f;     // 円の半径
        float speed = bulletSpeedMag*500.0f;      // 移動速度
        float angle = 0.0f;     // インボリュートの角度（θ）
        int involuteTimeMax = 3000;
        int count = 0;
        Vector2 collectorPos;
        collectorPos = GameObject.Find("PetalCollector").transform.position+((Vector3)startPos*2.0f);
        while (count < involuteTimeMax)
        {
            // インボリュートの現在の位置を計算し、基準位置に加算
            Vector2 involutePosition = CalculateInvolutePosition(radius, angle);
            transform.position = collectorPos + involutePosition;

            // 角度を更新して曲線上を移動
            angle += (speed * Time.deltaTime)*rotationWay;

            count++;
            yield return new WaitForEndOfFrame();
        }
        yield return homing();
    }
    private Vector2 CalculateInvolutePosition(float radius, float theta)
    {
        float x = radius * (Mathf.Cos(theta) + theta * Mathf.Sin(theta));
        float y = radius * (Mathf.Sin(theta) - theta * Mathf.Cos(theta));
        return new Vector2(x, y);
    }
    private IEnumerator spread(Vector3 way)
    {
        //指定された角度に展開する
        int count = 0;
        int countClock = 600;
        while (true)
        {
            count++;
            if (count >= countClock)
            {
                yield return homing();
            }
            transform.position += way * bulletSpeedMag;//可能であれば壁を認識したい。
            yield return new WaitForEndOfFrame();
        }

    }
    private IEnumerator homing()
    {
        int count = 0;
        int countClock = 300;
        //制限時間が終わるまでプレイヤーに向かって追尾する
        Vector3 moveWay = new Vector3(0, 0, 0);
        moveWay = GameObject.Find("Player").transform.position - transform.position;
        Vector3.Normalize(moveWay);
        while (true)
        {
            transform.position += (moveWay) * (bulletSpeedMag * 0.25f);
            moveWay = GameObject.Find("Player").transform.position - transform.position;
            Vector3.Normalize(moveWay);
            count++;
            if (count >= countClock)
            {
                yield return move();
            }
            yield return new WaitForEndOfFrame();
        }

    }
    private IEnumerator move()
    {//直進する。

        int count = 0;
        int countClock = 100;
        //制限時間が終わるまでプレイヤーに向かって追尾する
        Vector3 moveWay = new Vector3(0, 0, 0);
        moveWay = GameObject.Find("Player").transform.position - transform.position;
        Vector3.Normalize(moveWay);
        while (true)
        {
            transform.position += moveWay * bulletSpeedMag;
            count++;
            if (count >= countClock)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        cycleCount++;
        if (cycleCount > cycleCountMax)
        {
            Destroy(this.gameObject);
        }
        yield return homing();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }

    }
}
