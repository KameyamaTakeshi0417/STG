using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageUI3D : MonoBehaviour
{
    [SerializeField]
    private float DeleteTime = 1.0f;

    [SerializeField]
    private float MoveRange = 1000.0f;

    [SerializeField]
    private float EndAlpha = 0;

    private float TimeCnt;
    private TextMeshProUGUI NowText;
    public float damage;
    public float RandomPosition;

    // Start is called before the first frame update
    void Start()
    {
        RandomPosition = Random.Range(-50f, 50f);
        TimeCnt = 0.0f;
        Destroy(this.gameObject, DeleteTime);
        NowText = this.gameObject.GetComponent<TextMeshProUGUI>();
        this.transform.localPosition += new Vector3(RandomPosition, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        NowText.text = damage.ToString();

        // カメラ方向を向くが、反転を防ぐためにY軸の回転のみ調整
        Vector3 direction = Camera.main.transform.position - transform.position;
        direction.y = 0; // Y軸方向の回転を無視
        Quaternion lookRotation = Quaternion.LookRotation(-direction); // 反転防止のためマイナスを追加
        transform.rotation = lookRotation;

        TimeCnt += Time.deltaTime;
        this.gameObject.transform.position += new Vector3(
            0,
            MoveRange / DeleteTime * Time.deltaTime,
            0
        );

        float _alpha = 1.0f - (1.0f - EndAlpha) * (TimeCnt / DeleteTime);
        if (_alpha <= 0.0f)
            _alpha = 0.0f;
        NowText.color = new Color(NowText.color.r, NowText.color.g, NowText.color.b, _alpha);
    }
}
