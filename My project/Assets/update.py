import re

path = r'C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\UI\InventoryUI_Alpha.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = re.compile(r'        private void UpdateActiveEffectsDisplay\(\).*?        private void SetSlotVisual', re.DOTALL)

new_method = '''        private void UpdateActiveEffectsDisplay()
        {
            if (activeEffectsContainer == null || activeEffectPrefab == null || InventoryManager_Alpha.Instance == null) return;

            foreach (var go in spawnedActiveEffects)
            {
                Destroy(go);
            }
            spawnedActiveEffects.Clear();

            var activeEffects = InventoryManager_Alpha.Instance.GetActiveEffectsForDisplay();
            
            for (int currentGroup = -1; currentGroup <= 2; currentGroup++)
            {
                // Create Header
                GameObject headerGo = new GameObject($"Header_Group_{currentGroup}");
                headerGo.transform.SetParent(activeEffectsContainer, false);
                
                TextMeshProUGUI headerText = headerGo.AddComponent<TextMeshProUGUI>();
                TextMeshProUGUI sourceText = activeEffectPrefab.GetComponentInChildren<TextMeshProUGUI>();
                if (sourceText != null)
                {
                    headerText.font = sourceText.font;
                    headerText.fontSize = sourceText.fontSize;
                    headerText.color = sourceText.color;
                    headerText.alignment = TextAlignmentOptions.Center;
                }
                
                if (currentGroup == -1) headerText.text = "-全体効果-";
                else if (currentGroup == 0) headerText.text = "-グループ効果(1)-";
                else if (currentGroup == 1) headerText.text = "-グループ効果(2)-";
                else if (currentGroup == 2) headerText.text = "-グループ効果(3)-";
                
                LayoutElement headerLe = headerGo.AddComponent<LayoutElement>();
                headerLe.preferredHeight = 30f;
                headerLe.minHeight = 30f;
                
                spawnedActiveEffects.Add(headerGo);

                foreach (var info in activeEffects)
                {
                    if (info.groupIndex != currentGroup) continue;

                    var effect = info.effectSO;
                    int count = info.count;
                    float flatValue = info.flatValue;
                    
                    if (count <= 0 || effect == null) continue;

                    GameObject go = Instantiate(activeEffectPrefab, activeEffectsContainer, false);
                    go.SetActive(true);
                    spawnedActiveEffects.Add(go);
                    
                    RectTransform prefabRt = activeEffectPrefab.transform as RectTransform;
                    if (prefabRt != null)
                    {
                        LayoutElement le = go.GetComponent<LayoutElement>();
                        if (le == null) le = go.AddComponent<LayoutElement>();
                        le.preferredWidth = prefabRt.sizeDelta.x;
                        le.preferredHeight = prefabRt.sizeDelta.y;
                    }

                    TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        string colorTag = "";
                        string colorEnd = "";
                        
                        if (effect.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
                        {
                            bool isBoss = Alpha.Flow.StageManager_Alpha.Instance != null && Alpha.Flow.StageManager_Alpha.Instance.IsBossBattleActive;
                            if (isBoss)
                            {
                                colorTag = "<color=#FF4444>"; 
                                colorEnd = "</color>";
                            }
                            else
                            {
                                colorTag = "<color=#808080>"; 
                                colorEnd = "</color>";
                            }
                        }

                        text.text = $"・{colorTag}{count} \\\"{effect.effectName}\\\"{colorEnd}";
                    }

                    Image iconImg = null;
                    foreach (Transform child in go.transform)
                    {
                        if (child.name == "Icon")
                        {
                            iconImg = child.GetComponent<Image>();
                            break;
                        }
                    }
                    
                    if (iconImg == null)
                    {
                        foreach (Transform child in go.transform)
                        {
                            iconImg = child.GetComponent<Image>();
                            if (iconImg != null) break;
                        }
                    }

                    if (iconImg != null)
                    {
                        if (effect.effectIcon != null)
                        {
                            iconImg.sprite = effect.effectIcon;
                            iconImg.color = Color.white;
                        }
                    }

                    var rcd = go.AddComponent<RightClickDetector_Alpha>();
                    rcd.onRightClick = (eventData) => 
                    {
                        if (effectDetailPopup != null)
                        {
                            effectDetailPopup.Setup(effect, count, flatValue, eventData.position);
                        }
                    };
                }
            }
        }

        private void SetSlotVisual'''

content = pattern.sub(new_method, content)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)
print("Done")
