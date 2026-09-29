using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    /// <summary>
    /// SpriteRenderer用のAsepriteアニメーションをCanvas上のImageで再生するための補助スクリプト。
    /// 
    /// 【仕組み】
    /// AnimatorがSpriteRendererのspriteを毎フレーム書き換える
    /// → このスクリプトがそれをImageのspriteにコピーする
    /// → Canvas上でアニメーションが見える
    ///
    /// 【セットアップ】
    /// Canvas内のImageオブジェクトにこのスクリプトをアタッチするだけ。
    /// SpriteRendererは自動追加される。
    /// AnimatorはImageと同じオブジェクトにアタッチし、Asepriteのコントローラーを設定する。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Alpha_SyncSpriteToImage : MonoBehaviour
    {
        private Image img;
        private SpriteRenderer sr;

        void Awake()
        {
            img = GetComponent<Image>();
            sr = GetComponent<SpriteRenderer>();
            
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }

            // SpriteRendererは描画しない（Canvas上のImageだけで表示する）
            // enabled=falseにするとAnimatorがspriteを更新しなくなるので、
            // 代わりにalpha=0で透明にする
            sr.color = new Color(1f, 1f, 1f, 0f);
        }

        void LateUpdate()
        {
            if (img != null && sr != null && sr.sprite != null)
            {
                if (img.sprite != sr.sprite)
                {
                    img.sprite = sr.sprite;
                }
            }
        }
    }
}
