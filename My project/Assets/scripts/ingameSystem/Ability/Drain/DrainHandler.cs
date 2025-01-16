using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainHandler : _EffectHandlerBase
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public float DrainPoint = 0;

    public override void AttachEffect(GameObject targetObj)
    {
        DrainEffect targetScript = targetObj.AddComponent<DrainEffect>();
        targetScript.DrainPoint = DrainPoint;
    }
}
