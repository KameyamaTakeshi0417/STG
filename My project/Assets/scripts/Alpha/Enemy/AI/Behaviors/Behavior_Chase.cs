using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chase Behavior", menuName = "EnemyAI/Behaviors/Chase")]
public class Behavior_Chase : EnemyBehaviorData_Base
{
    public float speed = 3f;
    public float stopDistance = 0.5f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        while (true)
        {
            if (!ai.HasTarget())
            {
                ai.Rb.velocity = Vector2.zero;
                yield return new WaitForFixedUpdate();
                continue;
            }

            Vector2 toTarget = ai.TargetTransform.position - ai.transform.position;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                ai.Rb.velocity = toTarget.normalized * speed;
            }
            else
            {
                ai.Rb.velocity = Vector2.zero;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
