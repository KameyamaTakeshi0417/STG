using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class OrbControll_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    public int rarelity;
    private Transform playerTransform;
    private bool isCollected = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Collect(collision.gameObject);
        } 
        
    }
    void Start()
    {
        // 繝励Ξ繧､繝､繝ｼ繧貞､画焚縺ｫ繧ｭ繝｣繝・す繝･縺励※縺翫￥
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        SetupScatter();
    }
    public void SetupScatter()
    {
        StartCoroutine(Homing());
    }

    private IEnumerator Homing()
    {
        // 3. 謨｣繧峨・繧九い繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ
        float scatterDuration = 0.5f;
        float timer = 0f;
        Vector3 startPos = transform.position;
        // 繝ｩ繝ｳ繝繝縺ｪ譁ｹ蜷代∈謨｣繧峨・繧狗岼讓吩ｽ咲ｽｮ繧定ｨ育ｮ・
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 targetScatterPos = startPos + new Vector3(randomDir.x, randomDir.y, 0f) * Random.Range(1.0f, 2.0f);

        while (timer < scatterDuration)
        {
            if (isCollected) yield break; // 縺吶〒縺ｫ蝗槫庶縺輔ｌ縺ｦ縺・◆繧我ｸｭ譁ｭ

            timer += Time.deltaTime;
            // 繧､繝ｼ繧ｺ繧｢繧ｦ繝茨ｼ亥ｾ舌・↓貂幃溘☆繧九ｈ縺・↑蜍輔″・峨〒遘ｻ蜍・
            float t = timer / scatterDuration;
            t = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetScatterPos, t);
            yield return null;
        }
        if (!isCollected) transform.position = targetScatterPos;

        // 4. 1遘帝俣蠕・ｩ溘☆繧・
        yield return new WaitForSeconds(1.0f);

        // 5. 繝励Ξ繧､繝､繝ｼ縺ｫ蜷代°縺｣縺ｦ鬟帙ｓ縺ｧ縺・￥・医・繝ｼ繝溘Φ繧ｰ・・
        if (playerTransform == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) playerTransform = p.transform;
        }

        float homingSpeed = 2.0f;
        while (!isCollected && playerTransform != null)
        {
            // 譎る俣邨碁℃縺ｧ蠕舌・↓蜉騾溘☆繧・
            homingSpeed += Time.deltaTime * 15f;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, homingSpeed * Time.deltaTime);

            // 蠢ｵ縺ｮ縺溘ａ縲∬ｷ晞屬縺悟香蛻・↓霑代￠繧後・蝗槫庶蜃ｦ逅・ｒ蠑ｷ蛻ｶ逧・↓蜻ｼ縺ｶ
            if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
            {
                Collect(playerTransform.gameObject);
                break;
            }

            yield return null;
        }
    }

    private void Collect(GameObject playerObj)
    {
        if (!isCollected)
        {
            isCollected = true;
            if ((playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null) != null && (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null).GetComponent<treasureManager_Alpha>() != null)
            {
                (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null).GetComponent<treasureManager_Alpha>().GetTreasure(rarelity);
            }
        }
        // 遒ｺ螳溘↓遐ｴ螢翫☆繧・
        Destroy(gameObject);
    }

}
