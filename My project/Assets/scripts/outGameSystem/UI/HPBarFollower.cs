using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarFollower : MonoBehaviour
{
    public Transform targetObject; // 追従するターゲットオブジェクト
    public Vector3 offset=new Vector3(0, 1, 0); // ターゲットからのオフセット

    // Start is called before the first frame update
    void Start() { }

    public void setTargetTransform(Transform target)
    {
        targetObject = target;
    }

    private bool isOrphaned = false;

    // Update is called once per frame
    void Update()
    {
        bool hasTarget = targetObject != null && targetObject.gameObject.activeInHierarchy;

        if (hasTarget)
        {
            transform.position = targetObject.position + offset; 
        }
        else if (!isOrphaned)
        {
            isOrphaned = true;
            
            // エネミーが消滅・非アクティブになったら、HPバーは即座に非表示にする
            Transform hpBar = transform.Find("HPBar");
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
            
            // ダメージポップアップ（DamageUI3D）の演出完了を待ってからCanvasごと破棄する
            // DamageUI3DのDeleteTime(1.0f)より少し長めに設定
            Destroy(this.gameObject, 1.5f);
        }
    }
}
