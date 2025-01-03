using UnityEngine;
using UnityEngine.UI;

public class UIImageChanger : _ImageChanger_Base
{
    void OnEnable()
    {
        EquipManager.OnEquipChanged += ChangeTexture;
    }

    void OnDisable()
    {
        EquipManager.OnEquipChanged -= ChangeTexture;
    }

    public override void ChangeTexture(
        string changedItemCategory,
        string targetType,
        Sprite newSprite
    )
    {
        if (
            (changedItemCategory == itemCategory && targetType == itemType)
            && targetImage != null
            && newSprite != null
        )
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
