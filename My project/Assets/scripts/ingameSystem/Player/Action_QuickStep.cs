using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_QuickStep : MonoBehaviour
{
    public bool nowStep = false; // QuickStep使用中かどうか
    public float waitTime = 1.0f; // クールダウンの待機時間
    public float stepLength = 5.0f; // 移動する距離

    void Start()
    {
        nowStep = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !nowStep) // スペースキーでQuickStepを発動
        {
            StartCoroutine(startStep());
        }
    }

    private IEnumerator startStep()
    {
        nowStep = true; // QuickStepを開始
        Vector2 inputDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 入力がない場合は終了
        if (inputDirection == Vector2.zero)
        {
            nowStep = false;
            yield break;
        }

        yield return StartCoroutine(moveStep(inputDirection));
        yield return StartCoroutine(endStep());
    }

    private IEnumerator moveStep(Vector2 direction)
    {
        // 移動処理
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3)direction * stepLength;

        // 瞬間移動の処理
        transform.position = targetPosition;

        yield return null; // 瞬間移動なのでフレーム待機は不要
    }

    private IEnumerator endStep()
    {
        // クールダウン時間を待機
        yield return new WaitForSeconds(waitTime);
        nowStep = false; // QuickStepを再度使用可能にする
    }
}
