using UnityEngine;
using UnityEngine.UI;

public class UIImageChanger : MonoBehaviour
{
    public Image targetImage; // 対象の Image コンポーネント
    public Sprite newSprite;  // 新しい画像
    public string itemCategory; // アイテムのカテゴリ（Bullet, Case, Primerなど）

    void OnEnable()
    {
        EquipManager.OnEquipChanged += ChangeTextureByEquipManager;
    }

    void OnDisable()
    {
        EquipManager.OnEquipChanged -= ChangeTextureByEquipManager;
    }

    public void ChangeTextureByEquipManager(string changedItemCategory, Sprite newSprite)
    {
        if (changedItemCategory == itemCategory && targetImage != null && newSprite != null)
        {
            // Image コンポーネントの Source Image を newSprite に変更
            targetImage.sprite = newSprite;
            Debug.Log("Image successfully changed to new sprite for category: " + itemCategory);
        }
        else if (changedItemCategory != itemCategory)
        {
            Debug.Log("No matching category found for update. Category: " + changedItemCategory);
        }
        else
        {
            Debug.LogWarning("Target image or new sprite is not set.");
        }
    }
}
