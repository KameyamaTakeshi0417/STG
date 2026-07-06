using UnityEngine; using UnityEditor; [InitializeOnLoad] public class PhysicsTest { static PhysicsTest() { Debug.Log("Enemy vs Enemy Ignore: " + Physics2D.GetIgnoreLayerCollision(9, 9)); } }
