using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarFollower : MonoBehaviour
{
    public Transform targetObject; // 追従するターゲットオブジェクト

    // Start is called before the first frame update
    void Start() { }

    public void setTargetTransform(Transform target)
    {
        targetObject = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetObject != null)
        {
            transform.position = targetObject.position + new Vector3(0, 1, 0); // ターゲットの上にオフセットを追加
            //transform.LookAt(Camera.main.transform); // カメラの方向を向く
        }
        if (targetObject == null)
        {
            Destroy(this.gameObject);
        }
    }
}
