using UnityEngine;

public class Alpha_Enemy_Movement : MonoBehaviour
{
    [Header("Movement Toggles (Flags)")]
    [Tooltip("読み込み時の地点を中心に左右に移動する")]
    public bool isHorizontalMovement = false;
    
    [Tooltip("読み込み時の地点を中心に衛星軌道（円）を描いて移動する")]
    public bool isOrbitalMovement = false;

    [Header("Movement Parameters")]
    public float moveRadius = 3f;      // 移動半径・左右の移動幅
    public float moveSpeed = 2f;       // 移動速度の倍率
    public float returnSpeed = 5f;     // 位置復帰時および補間(Lerp)時のスムーズスピード

    protected Vector3 initialPosition; // 読み込み時の初期位置
    
    // 内部ステート管理用
    private enum State { Idle, Returning, Moving }
    private State currentState = State.Idle;
    
    // 現在アクティブな移動タイプを記憶（Inspector等で途中で変更されたかチェック用）
    private bool wasHorizontal = false;
    private bool wasOrbital = false;

    // 動作の進行度（サイン波などの時間経過計算・角度用）
    private float moveProgress = 0f;

    protected virtual void Start()
    {
        // 読み込み時地点を記録
        initialPosition = transform.position;
        currentState = State.Idle;
    }

    protected virtual void Update()
    {
        // フラグ状態の監視とステートの切り替え
        CheckMovementToggles();

        switch (currentState)
        {
            case State.Idle:
                // 移動がONになっていないときは何もしない
                break;
            case State.Returning:
                ProcessReturnToStart();
                break;
            case State.Moving:
                if (isHorizontalMovement)
                {
                    ProcessHorizontalMovement();
                }
                else if (isOrbitalMovement)
                {
                    ProcessOrbitalMovement();
                }
                break;
        }
    }

    private void CheckMovementToggles()
    {
        // もしフラグがUnityエディタ等で途中で切り替わったら
        if (isHorizontalMovement != wasHorizontal || isOrbitalMovement != wasOrbital)
        {
            wasHorizontal = isHorizontalMovement;
            wasOrbital = isOrbitalMovement;

            if (!isHorizontalMovement && !isOrbitalMovement)
            {
                // 全てのフラグがオフになったら停止
                currentState = State.Idle;
            }
            else
            {
                // 新しい移動がONになったら、まずは開始想定地点(あるいは軌道上)までスムーズに戻る
                currentState = State.Returning;
                
                // 次の運動の経過時間をリセット（スムーズに最初から計算するため）
                moveProgress = 0f;
            }
        }
    }

    private void ProcessReturnToStart()
    {
        // 現在設定されている動作の「開始想定位置」を計算
        Vector3 targetPos = initialPosition;
        if (isHorizontalMovement)
        {
            // 水平移動の開始位置は「初期地点」 (Sin(0) = 0のため)
            targetPos = initialPosition;
        }
        else if (isOrbitalMovement)
        {
            // 軌道移動の開始位置は「初期地点から右にmoveRadius進んだ場所」 (Cos(0)*r, Sin(0)*rのため)
            targetPos = initialPosition + new Vector3(moveRadius, 0, 0);
        }

        // 線形補間(Lerp)でスムーズに戻る
        transform.position = Vector3.Lerp(transform.position, targetPos, returnSpeed * Time.deltaTime);

        // 目的地に十分近づいたら、正式な移動ステートへ移行
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            transform.position = targetPos; // ズレを補正してピッタリ合わせる
            currentState = State.Moving;
        }
    }

    private void ProcessHorizontalMovement()
    {
        // 進行度を進める
        moveProgress += moveSpeed * Time.deltaTime;

        // Mathf.Sinで -1.0 〜 1.0 のなめらかな波（往復値）を作る
        float offset = Mathf.Sin(moveProgress) * moveRadius;

        // 次に行くべき目標位置をセット
        Vector3 targetPos = initialPosition + new Vector3(offset, 0, 0);

        // 線形補間(Lerp)で目標にスムーズに追従させる
        transform.position = Vector3.Lerp(transform.position, targetPos, returnSpeed * Time.deltaTime);
    }

    private void ProcessOrbitalMovement()
    {
        // 進行度を進める (ラジアン角度として使用)
        moveProgress += moveSpeed * Time.deltaTime;

        // 円軌道の計算 (XはCos, YはSin)
        float x = Mathf.Cos(moveProgress) * moveRadius;
        float y = Mathf.Sin(moveProgress) * moveRadius;

        // 次に行くべき目標位置
        Vector3 targetPos = initialPosition + new Vector3(x, y, 0);

        // 線形補間(Lerp)で目標にスムーズに追従させる
        transform.position = Vector3.Lerp(transform.position, targetPos, returnSpeed * Time.deltaTime);
    }
}
