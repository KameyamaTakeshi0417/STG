using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Alpha.Data;

public class EquipUIManager_Alpha : MonoBehaviour
{
    public InventoryManager_Alpha inventoryManager;
    
    [Header("Highlight Settings")]
    [Tooltip("アクティブな武器スロットを強調する色")]
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    [Tooltip("非アクティブ時の色")]
    public Color normalColor = Color.white;
    [Tooltip("枠や背景自体の色も変更するかどうか（鉢の背景Imageなど）")]
    public bool changeParentColor = true;

    [Header("Stack & Slide Settings")]
    [Tooltip("各セットの親オブジェクト（鉢）")]
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
    
    // キャッシュ用ハッシュ（セットごとに保持）
    private int[] lastEquipHashes = new int[3] { -1, -1, -1 };

    void Update()
    {
        if (inventoryManager != null)
        {
            for (int i = 0; i < 3; i++)
            {
                int currentHash = GetGroupHash(i);
                if (currentHash != lastEquipHashes[i])
                {
                    lastEquipHashes[i] = currentHash;
                    RebuildPlantUI(i);
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

        for (int i = 0; i < setTransforms.Length; i++)
        {
            if (setTransforms[i] == null) continue;

            bool isActive = (i == activeGroup) || isBouquet;
            Color colorToApply = isActive ? highlightColor : normalColor;

            Image rootImg = setTransforms[i].GetComponent<Image>();
            if (changeParentColor && rootImg != null)
            {
                rootImg.color = colorToApply;
            }

            // 生成された植物パーツの色を更新
            Image[] childImages = setTransforms[i].GetComponentsInChildren<Image>();
            foreach (var img in childImages)
            {
                if (img.gameObject == setTransforms[i].gameObject) continue;
                img.color = colorToApply;
            }
        }

        // セット全体のスタック・スライド・回転処理
        UpdateSetTransforms(activeGroup, isBouquet);
    }

    private int GetGroupHash(int groupIndex)
    {
        if (inventoryManager == null || inventoryManager.equipInstance == null) return -1;
        
        int hash = 17;
        int startIndex = groupIndex * 3;
        for (int j = 0; j < 3; j++)
        {
            int index = startIndex + j;
            if (index < inventoryManager.equipInstance.Count)
            {
                var inst = inventoryManager.equipInstance[index];
                hash = hash * 31 + (inst.series != null ? inst.series.GetInstanceID() : 0);
            }
            else
            {
                hash = hash * 31;
            }
        }
        return hash;
    }

    private void RebuildPlantUI(int groupIndex)
    {
        if (groupIndex >= setTransforms.Length || setTransforms[groupIndex] == null) return;
        
        RectTransform root = setTransforms[groupIndex];
        
        // 既存の動的生成レイヤー（_Layerで終わるもの）のみをクリア
        // こうすることで、ユーザーが独自に追加した枠やMask等のオブジェクトは削除されません
        foreach (Transform child in root)
        {
            if (child.name.EndsWith("_Layer"))
            {
                Destroy(child.gameObject);
            }
        }

        int startIndex = groupIndex * 3;
        if (startIndex + 2 >= inventoryManager.equipInstance.Count) return;

        // InventoryManagerの仕様: 0=Primer(雷管), 1=Casing(薬莢), 2=Bullet(弾頭)
        WeaponSeriesData_Alpha primerSeries = inventoryManager.equipInstance[startIndex].series;
        WeaponSeriesData_Alpha casingSeries = inventoryManager.equipInstance[startIndex + 1].series;
        WeaponSeriesData_Alpha bulletSeries = inventoryManager.equipInstance[startIndex + 2].series;

        // レイヤー生成（描画順：背景(Root) < Primer(茎根) < Casing(草・葉) < Bullet(花)）
        Transform primerLayer = CreateLayer("Primer_Layer", root);
        Transform casingLayer = CreateLayer("Casing_Layer", root);
        Transform bulletLayer = CreateLayer("Bullet_Layer", root);

        // 茎根（Primer）の生成
        if (primerSeries != null && primerSeries.iconPrimer != null)
        {
            GameObject primerObj = CreateImageObj("Primer", primerLayer, primerSeries.iconPrimer, primerSeries.pivotPrimer, primerSeries.scalePrimer, Vector2.zero);
            RectTransform primerRT = primerObj.GetComponent<RectTransform>();

            // 葉（Casing）の生成
            if (casingSeries != null && casingSeries.iconCasing != null)
            {
                var leafPoints = primerSeries.leafAttachmentPoints;
                if (leafPoints == null || leafPoints.Count == 0) leafPoints = new List<Vector2> { new Vector2(0.5f, 1.0f) };

                foreach (var lp in leafPoints)
                {
                    Vector2 casingPos = CalculateAttachmentOffset(primerRT, primerSeries.pivotPrimer, lp, primerSeries.scalePrimer);
                    CreateImageObj("Casing", casingLayer, casingSeries.iconCasing, casingSeries.pivotCasing, casingSeries.scaleCasing, casingPos);
                }
            }

            // 花（Bullet）の生成
            if (bulletSeries != null && bulletSeries.iconBullet != null)
            {
                var flowerPoints = primerSeries.flowerAttachmentPoints; // 茎根から取得
                if (flowerPoints == null || flowerPoints.Count == 0) flowerPoints = new List<Vector2> { new Vector2(0.5f, 1.0f) };

                foreach (var fp in flowerPoints)
                {
                    Vector2 bulletPos = CalculateAttachmentOffset(primerRT, primerSeries.pivotPrimer, fp, primerSeries.scalePrimer); // 茎根からのオフセット
                    CreateImageObj("Bullet", bulletLayer, bulletSeries.iconBullet, bulletSeries.pivotBullet, bulletSeries.scaleBullet, bulletPos);
                }
            }
        }
        else
        {
            // Primerが無い場合のフォールバック（単体表示など）
            if (casingSeries != null && casingSeries.iconCasing != null)
            {
                CreateImageObj("Casing", casingLayer, casingSeries.iconCasing, casingSeries.pivotCasing, casingSeries.scaleCasing, Vector2.zero);
            }
            if (bulletSeries != null && bulletSeries.iconBullet != null)
            {
                CreateImageObj("Bullet", bulletLayer, bulletSeries.iconBullet, bulletSeries.pivotBullet, bulletSeries.scaleBullet, Vector2.zero);
            }
        }
    }

    private Transform CreateLayer(string layerName, Transform parent)
    {
        GameObject go = new GameObject(layerName, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        // 親(鉢)と同じサイズに合わせる
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private GameObject CreateImageObj(string name, Transform parent, Sprite sprite, Vector2 pivot, Vector2 scale, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        
        rt.pivot = pivot;
        rt.localScale = new Vector3(scale.x, scale.y, 1f);
        rt.anchoredPosition = anchoredPos;
        
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize(); // 元画像のサイズに合わせる
        
        return go;
    }

    private Vector2 CalculateAttachmentOffset(RectTransform parentRT, Vector2 parentPivot, Vector2 attachmentNormalized, Vector2 parentScale)
    {
        Vector2 size = parentRT.rect.size;
        float offsetX = (attachmentNormalized.x - parentPivot.x) * size.x * parentScale.x;
        float offsetY = (attachmentNormalized.y - parentPivot.y) * size.y * parentScale.y;
        return new Vector2(offsetX, offsetY);
    }

    private void UpdateSetTransforms(int activeGroup, bool isBouquet)
    {
        if (setTransforms == null || setTransforms.Length == 0) return;

        for (int i = 0; i < setTransforms.Length; i++)
        {
            if (setTransforms[i] == null) continue;

            int depth = (i - activeGroup + setTransforms.Length) % setTransforms.Length;
            
            Vector2 targetPos = Vector2.zero;
            float targetAngle = 0f;

            if (isBouquet)
            {
                // オムニブーケ発動中：扇状に展開
                targetPos = Vector2.zero; 
                if (i < bouquetAngles.Length)
                {
                    targetAngle = bouquetAngles[i];
                }
            }
            else
            {
                // 通常状態：スタックとスライド
                targetPos = offsetPerDepth * depth;
                targetAngle = anglePerDepth * depth; // Z軸回転
                
                if (depth == 0)
                {
                    setTransforms[i].SetAsLastSibling();
                }
                else if (depth == 1)
                {
                    setTransforms[i].SetSiblingIndex(setTransforms[i].parent.childCount - 2);
                }
                else if (depth == 2)
                {
                    setTransforms[i].SetAsFirstSibling();
                }
            }

            setTransforms[i].anchoredPosition = Vector2.Lerp(setTransforms[i].anchoredPosition, targetPos, Time.deltaTime * lerpSpeed);
            
            Quaternion currentRot = setTransforms[i].localRotation;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            setTransforms[i].localRotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * lerpSpeed);
        }
    }
}
