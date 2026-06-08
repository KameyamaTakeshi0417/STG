using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerinShooting : Player
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
    protected override void FixedUpdate()
    {
        var health = GetComponent<_Health_Base>();
        if (health != null && health.isStunned)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 入力がない場合は何もしない
        Vector2 input = Vector2.zero;
        if (Alpha.Core.InputManager_Alpha.Instance != null)
        {
            input = new Vector2(Alpha.Core.InputManager_Alpha.Instance.GetAxisRaw("Horizontal"), Alpha.Core.InputManager_Alpha.Instance.GetAxisRaw("Vertical"));
        }
        else
        {
            // InputManagerがない場合のフォールバック（テストシーン用など）
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        
        if (input == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 入力の正規化
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }

        // キャラクターを移動させる
        float setSpd = (moveSpeed * moveSpeedMag);
        if (setSpd <= 0)
        {
            setSpd = 0.1f;
        }
        rb.velocity = input * setSpd;
    }

}
