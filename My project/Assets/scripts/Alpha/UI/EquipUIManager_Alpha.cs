using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipUIManager_Alpha : MonoBehaviour
{
    public InventoryManager_Alpha inventoryManager;
    public Image[] equipUIs;
    public Sprite emptyImage;
    [Header("Highlight Settings")]
    [Tooltip("アクティブな武器スロットを強調する色")]
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    [Tooltip("非アクティブ時の色")]
    public Color normalColor = Color.white;
    [Tooltip("アイコン自身ではなく、親オブジェクト（枠やマスク）の色を変更するかどうか")]
    public bool changeParentColor = true;

    [Header("Stack & Slide Settings")]
    [Tooltip("各セット(0-2, 3-5, 6-8)の親オブジェクト")]
    public RectTransform[] setTransforms = new RectTransform[3];
    
    [Tooltip("アニメーションの速度")]
    public float lerpSpeed = 10f;
    
    [Tooltip("重なっている時に背面のセットをどれくらいズラすか")]
    public Vector2 offsetPerDepth = new Vector2(10f, -10f);
    
    [Tooltip("重なっている時に背面のセットをどれくらい傾けるか")]
    public float anglePerDepth = 15f;

    [Header("Omni-Bouquet Settings")]
    [Tooltip("オムニブーケ発動時の各セットの角度 (0, 45, 90など)")]
    public float[] bouquetAngles = new float[] { 0f, 45f, 90f };

    private Player_Shooter_Alpha playerShooter;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // インベントリの変更を毎フレーム反映する
        if (inventoryManager != null)
        {
            for(int i = 0; i < equipUIs.Length; i++)
            {
                if (equipUIs[i] == null) continue;

                if (i < inventoryManager.equipInstance.Count)
                {
                    var instance = inventoryManager.equipInstance[i];
                    if (instance.series != null && instance.series.icon != null)
                    {
                        equipUIs[i].sprite = instance.series.icon;
                        equipUIs[i].enabled = true;
                    }
                    else
                    {
                        equipUIs[i].sprite = emptyImage;
                    }
                }
                else
                {
                    equipUIs[i].sprite = emptyImage;
                }
            }
        }

        if (playerShooter == null)
        {
            playerShooter = FindAnyObjectByType<Player_Shooter_Alpha>();
            if (playerShooter == null) return;
        }

        int activeGroup = playerShooter.currentWeaponGroup;
        bool isBouquet = inventoryManager != null && inventoryManager.IsBouquetActive();

        for (int i = 0; i < equipUIs.Length; i++)
        {
            if (equipUIs[i] == null) continue;

            // スロットのインデックスから所属グループを判定
            int groupIndex = (equipUIs.Length == 3) ? i : (i / 3);
            bool isActive = (groupIndex == activeGroup) || isBouquet; // ブーケ中は全アクティブ扱いでも良いが色は変更

            // 色を変更する対象のImageを取得
            Image targetImage = equipUIs[i];
            if (changeParentColor && equipUIs[i].transform.parent != null)
            {
                Image parentImage = equipUIs[i].transform.parent.GetComponent<Image>();
                if (parentImage != null)
                {
                    targetImage = parentImage;
                }
            }

            if (targetImage != null)
            {
                targetImage.color = (groupIndex == activeGroup || isBouquet) ? highlightColor : normalColor;
            }
        }

        // セット全体のスタック・スライド・回転処理
        UpdateSetTransforms(activeGroup, isBouquet);
    }

    private void UpdateSetTransforms(int activeGroup, bool isBouquet)
    {
        if (setTransforms == null || setTransforms.Length == 0) return;

        // Depth(深さ)の計算用。最前面(0) -> 中央(1) -> 一番後ろ(2)
        // 例えばactiveGroupが0の時、0->0, 1->1, 2->2
        // activeGroupが1の時、0->2, 1->0, 2->1
        
        for (int i = 0; i < setTransforms.Length; i++)
        {
            if (setTransforms[i] == null) continue;

            int depth = (i - activeGroup + setTransforms.Length) % setTransforms.Length;
            
            Vector2 targetPos = Vector2.zero;
            float targetAngle = 0f;

            if (isBouquet)
            {
                // オムニブーケ発動中：扇状に展開
                targetPos = Vector2.zero; // Pivotを中心に開くのでオフセットはゼロ
                if (i < bouquetAngles.Length)
                {
                    targetAngle = bouquetAngles[i];
                }
                
                // ブーケ中はZオーダー(重なり順)は固定か、そのままにする
                // ここではすべて最前面に近い扱いにしたいが、一旦そのままの順序
            }
            else
            {
                // 通常状態：スタックとスライド
                targetPos = offsetPerDepth * depth;
                targetAngle = anglePerDepth * depth; // Z軸回転
                
                // Depthが0（最前面）のものは、Hierarchyで最後に移動させて前に描画する
                // （※Updateで毎フレーム呼ぶと重い場合はキャッシュ判定を入れる）
                if (depth == 0)
                {
                    setTransforms[i].SetAsLastSibling();
                }
                else if (depth == 1)
                {
                    // 1つ後ろ
                    // 最前面より前に出ないように、適宜SiblingIndexを調整
                    setTransforms[i].SetSiblingIndex(setTransforms[i].parent.childCount - 2);
                }
                else if (depth == 2)
                {
                    // 2つ後ろ（一番後ろ）
                    setTransforms[i].SetAsFirstSibling();
                }
            }

            // Lerpで滑らかにアニメーション
            setTransforms[i].anchoredPosition = Vector2.Lerp(setTransforms[i].anchoredPosition, targetPos, Time.deltaTime * lerpSpeed);
            
            Quaternion currentRot = setTransforms[i].localRotation;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            setTransforms[i].localRotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * lerpSpeed);
        }
    }
}
