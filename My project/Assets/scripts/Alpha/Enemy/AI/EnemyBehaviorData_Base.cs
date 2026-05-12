using System.Collections;
using UnityEngine;

public abstract class EnemyBehaviorData_Base : ScriptableObject
{
    // 各挙動ごとのコルーチン処理を定義する。ローカル変数はコルーチン内に保持されるため、安全に共有可能。
    public abstract IEnumerator RunBehavior(Alpha_EnemyAI ai);
}
