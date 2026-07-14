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
    [Tooltip("植物パーツ全体の表示倍率（フレームに対して画像が大きすぎる場合は小さくする）")]
    public float globalPlantScale = 1.0f;
    
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
    
    private int[] lastEquipHashes = new int[3] { -1, -1, -1 };
    
    private Vector2 basePosition;

    void Start()
    {
        if (setTransforms.Length > 0 && setTransforms[0] != null)
        {
            // エディタ上で配置した1stFrameの初期位置を、アニメーションの集合基準位置とする
            basePosition = setTransforms[0].anchoredPosition;
        }

        // フレーム（カード）のアニメーション中心位置を「下端（0.5, 0）」に強制補正する
        for (int i = 0; i < setTransforms.Length; i++)
        {
            if (setTransforms[i] != null)
            {
                SetPivotKeepPosition(setTransforms[i], new Vector2(0.5f, 0f));
            }
        }
    }

    // 見た目の位置をズラさずにPivotだけを変更する便利関数
    private void SetPivotKeepPosition(RectTransform rt, Vector2 newPivot)
    {
        Vector2 size = rt.rect.size;
        Vector2 deltaPivot = rt.pivot - newPivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x * rt.localScale.x, deltaPivot.y * size.y * rt.localScale.y, 0f);
        
        rt.pivot = newPivot;
        rt.localPosition -= rt.rotation * deltaPosition;
    }

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
        
        // 古いレイヤーを確実に削除（即座に親から外すことで表示バグを防ぐ）
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in root)
        {
            if (child.name == "Primer_Layer" || child.name == "Casing_Layer" || child.name == "Bullet_Layer")
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (var go in toDestroy)
        {
            go.transform.SetParent(null);
            Destroy(go);
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

        // スクリプト生成の植物レイヤーを階層の一番上（描画順では一番奥）に移動させる
        // こうすることで、ユーザーが独自に追加した枠線（frame_Layerなど）が植物の手前に描画されます
        primerLayer.SetSiblingIndex(0);
        casingLayer.SetSiblingIndex(1);
        bulletLayer.SetSiblingIndex(2);

        // 茎根（Primer）の生成
        if (primerSeries != null && primerSeries.iconPrimer != null)
        {
            GameObject primerObj = CreateImageObj("Primer", primerLayer, primerSeries.iconPrimer, primerSeries.scalePrimer, Vector2.zero);
            RectTransform primerRT = primerObj.GetComponent<RectTransform>();

            // 葉（Casing）の生成
            if (casingSeries != null && casingSeries.iconCasing != null)
            {
                var leafPoints = primerSeries.leafAttachmentPoints;
                if (leafPoints == null || leafPoints.Count == 0) leafPoints = new List<Vector2> { new Vector2(0.5f, 1.0f) };

                foreach (var lp in leafPoints)
                {
                    Vector2 casingPos = CalculateAttachmentOffset(primerRT, lp, primerSeries.scalePrimer);
                    CreateImageObj("Casing", casingLayer, casingSeries.iconCasing, casingSeries.scaleCasing, casingPos);
                }
            }

            // 花（Bullet）の生成
            if (bulletSeries != null && bulletSeries.iconBullet != null)
            {
                var flowerPoints = primerSeries.flowerAttachmentPoints; // 茎根から取得
                if (flowerPoints == null || flowerPoints.Count == 0) flowerPoints = new List<Vector2> { new Vector2(0.5f, 1.0f) };

                foreach (var fp in flowerPoints)
                {
                    Vector2 bulletPos = CalculateAttachmentOffset(primerRT, fp, primerSeries.scalePrimer); // 茎根からのオフセット
                    CreateImageObj("Bullet", bulletLayer, bulletSeries.iconBullet, bulletSeries.scaleBullet, bulletPos);
                }
            }
        }
        else
        {
            // Primerが無い場合のフォールバック（単体表示など）
            if (casingSeries != null && casingSeries.iconCasing != null)
            {
                CreateImageObj("Casing", casingLayer, casingSeries.iconCasing, casingSeries.scaleCasing, Vector2.zero);
            }
            if (bulletSeries != null && bulletSeries.iconBullet != null)
            {
                CreateImageObj("Bullet", bulletLayer, bulletSeries.iconBullet, bulletSeries.scaleBullet, Vector2.zero);
            }
        }
    }

    private Transform CreateLayer(string layerName, Transform parent)
    {
        GameObject go = new GameObject(layerName, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        
        // レイヤー自体も親(鉢)の下部中央 (0.5, 0) を基準点とする
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        
        // サイズを0にし、オフセットを持たない純粋な「基準点」として機能させる
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        
        // 全体スケールを適用
        rt.localScale = new Vector3(globalPlantScale, globalPlantScale, 1f);
        
        return rt;
    }

    // pivot引数を廃止し、強制的に(0.5, 0)で生成するように修正
    private GameObject CreateImageObj(string name, Transform parent, Sprite sprite, Vector2 scale, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        
        // アンカーとピボットを完全に「下部中央 (0.5, 0)」に固定
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        
        rt.localScale = new Vector3(scale.x, scale.y, 1f);
        rt.anchoredPosition = anchoredPos;
        
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize(); // 元画像のサイズに合わせる
        
        return go;
    }

    // 親のピボットが必ず(0.5, 0)であることを前提にした計算に変更
    private Vector2 CalculateAttachmentOffset(RectTransform parentRT, Vector2 attachmentNormalized, Vector2 parentScale)
    {
        Vector2 size = parentRT.rect.size;
        float offsetX = (attachmentNormalized.x - 0.5f) * size.x * parentScale.x;
        float offsetY = (attachmentNormalized.y - 0f) * size.y * parentScale.y;
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
                targetPos = basePosition; 
                if (i < bouquetAngles.Length)
                {
                    targetAngle = bouquetAngles[i];
                }
            }
            else
            {
                // 通常状態：スタックとスライド
                targetPos = basePosition + (offsetPerDepth * depth);
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
