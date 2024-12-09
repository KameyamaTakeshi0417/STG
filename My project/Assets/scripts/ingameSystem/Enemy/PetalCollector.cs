using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalCollector : MonoBehaviour
{
    public Vector3 rotate; //プレイヤーに向かった時の角度
    public float reachForward; //遠いかどうか
    public float reachBehind; //近いかどうか
    public float moveMag;

    // Start is called before the first frame update
    void Awake()
    {
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
        int limitClock = 500; //待機カウント
        int count = 0;
        while (true)
        {
            count++;
            gameObject.transform.position +=
                (Vector3.Normalize(checkDirectionToPlayer())) * moveMag;

            if (checkDirectionToPlayer().magnitude < reachForward)
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

    private IEnumerator attack0()
    { //牽制技
        int[] shootSet = new int[3];
        shootSet[0] = 6;
        shootSet[1] = 2;
        shootSet[2] = 10;
        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet.GetComponent<petalBullet>().shoot(shootSet[i]);
            yield return new WaitForEndOfFrame();
        }
        yield return attack1();
    }

    private IEnumerator attack1()
    { //通常攻撃
        GameObject bullet;
        GameObject bullet2;
        float bulletShootSpeed = 0.3f;
        for (int i = 0; i < 50; i++)
        {
            bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet.GetComponent<petalBullet>().ShootInvolute(2, true);

            bullet2 = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet2.GetComponent<petalBullet>().ShootInvolute(10, false);
            yield return new WaitForSeconds(bulletShootSpeed);
        }
        for (int i = 0; i < 50; i++)
        {
            bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet.GetComponent<petalBullet>().ShootInvolute(6, true);

            bullet2 = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet2.GetComponent<petalBullet>().ShootInvolute(0, false);
            yield return new WaitForSeconds(bulletShootSpeed);
        }
        for (int i = 0; i < 50; i++)
        {
            bullet = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet.GetComponent<petalBullet>().ShootInvolute(4, true);

            bullet2 = Instantiate(
                Resources.Load<GameObject>("Objects/Bullet/petalBullet"),
                gameObject.transform.position,
                Quaternion.identity
            );
            bullet2.GetComponent<petalBullet>().ShootInvolute(8, false);
            yield return new WaitForSeconds(bulletShootSpeed);
        }
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
}
