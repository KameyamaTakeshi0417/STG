using UnityEngine;

namespace Alpha.Environment
{
    public class GrassInteractor_Alpha : MonoBehaviour
    {
        // シェーダー内で定義したグローバル変数名
        private static readonly int PlayerPosProp = Shader.PropertyToID("_PlayerPos");

        private void Update()
        {
            // 毎フレーム、自身の座標を全シェーダーの_PlayerPosに共有する
            Shader.SetGlobalVector(PlayerPosProp, transform.position);
        }
    }
}
