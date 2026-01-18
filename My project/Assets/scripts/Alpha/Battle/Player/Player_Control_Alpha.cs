using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Control_Alpha : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator animator; // Animator コンポーネントを追加
    public bool onCoolTime;
    playerStatusManager_Alpha myStatus;

    // Start is called before the first frame update

    void Start()
    {
        onCoolTime = false;
        myStatus=GameObject.Find("manager").GetComponent<playerStatusManager_Alpha>();
    }
    protected virtual void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Animator を取得
        
    }
    // Update is called once per frame

    protected virtual void Update()
    {
        if (Time.timeScale == 0f)
            return;

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // ロックオン対象がいる場合、その方向に向ける

        // Animator パラメータの更新
        UpdateAnimatorParameters();
    }
    protected virtual void FixedUpdate()
    {

        // 入力がない場合は何もしない
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftShift)) {
                //ここに入れたい
                rb.angularVelocity = 0f;   // 念のため（回転してないなら不要）
                rb.position = rb.position; // 実質変化なしだけど、直後の処理でズレるなら使う
                rb.Sleep();                // 物理的に「寝かせて」微振動/押し出しを止める
            }
            return;
        }

        // 入力の正規化
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }

        // キャラクターを移動させる
        float setSpd = (myStatus.moveSpeed * myStatus.moveSpeedMag *0.0001f);
        rb.velocity = input * setSpd * myStatus.moveSpeedMag_CONST;
        if (setSpd <= 0 || Input.GetKey(KeyCode.LeftShift))
        {
            rb.velocity = input*0.79f;
        }
        
       
        
    }

    protected virtual void UpdateAnimatorParameters()
    {
        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // マウス位置のX座標をAnimatorパラメータに設定
        float mouseXPosition = rb.velocity.x; // mousePosition.x;

        // プレイヤーの移動ベクトルの大きさを計算
        float moveVectorMag = rb.velocity.magnitude;

        // Animator パラメータを設定
        animator.SetFloat("mouseXPosition", mouseXPosition);
        animator.SetFloat("moveVectorMag", moveVectorMag);
    }
}
