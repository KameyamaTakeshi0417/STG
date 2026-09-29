using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class P {
    static void Main() {
        string path = @"C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\UI\InventoryUI_Alpha.cs";
        var lines = new List<string>(File.ReadAllLines(path, Encoding.UTF8));

        // 1. Add imports at line 1 (index 0): insert "using System.Collections;" and "using DG.Tweening;"
        lines.Insert(0, "using System.Collections;");
        // After insertion, "using System.Collections.Generic;" is now at index 2
        // Find "using Alpha.Data;" and insert after it
        for (int i = 0; i < lines.Count; i++) {
            if (lines[i].Trim() == "using Alpha.Data;") {
                lines.Insert(i + 1, "using DG.Tweening;");
                break;
            }
        }

        // 2. Find "private bool isReadOnly = false;" and add animation fields after it
        for (int i = 0; i < lines.Count; i++) {
            if (lines[i].Contains("private bool isReadOnly = false;")) {
                var newFields = new string[] {
                    "",
                    "        // Animation fields",
                    "        private CanvasGroup panelCanvasGroup;",
                    "        private Vector3 panelOriginalScale = Vector3.one;",
                    "        private bool isCloseAnimating = false;"
                };
                lines.InsertRange(i + 1, newFields);
                break;
            }
        }

        // 3. Replace Hide() method - find "public void Hide()" and replace until closing brace
        for (int i = 0; i < lines.Count; i++) {
            if (lines[i].Contains("public void Hide()")) {
                // Find closing brace of this method
                int braceCount = 0;
                int start = i;
                int end = i;
                for (int j = i; j < lines.Count; j++) {
                    if (lines[j].Contains("{")) braceCount++;
                    if (lines[j].Contains("}")) braceCount--;
                    if (braceCount == 0 && j > i) { end = j; break; }
                }
                // Remove old Hide
                lines.RemoveRange(start, end - start + 1);
                // Insert new Hide
                var newHide = new string[] {
                    "        public void Hide()",
                    "        {",
                    "            if (detailPopup != null) detailPopup.gameObject.SetActive(false);",
                    "            if (effectDetailPopup != null) effectDetailPopup.Hide();",
                    "            if (confirmButton != null) confirmButton.gameObject.SetActive(false);",
                    "            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);",
                    "            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);",
                    "            PlayCloseAnimation(null);",
                    "        }"
                };
                lines.InsertRange(start, newHide);
                break;
            }
        }

        // 4. Add PlayOpenAnimation() call after each RefreshUI() in Show methods
        // We look for "RefreshUI();" followed by "TryShowEquipTutorial();" or "PlayOpenAnimation();" pattern
        for (int i = 0; i < lines.Count; i++) {
            if (lines[i].Trim() == "RefreshUI();") {
                // Check if next non-empty line is TryShowEquipTutorial or PlayOpenAnimation already
                int nextLine = i + 1;
                while (nextLine < lines.Count && string.IsNullOrWhiteSpace(lines[nextLine])) nextLine++;
                if (nextLine < lines.Count && lines[nextLine].Contains("TryShowEquipTutorial")) {
                    // Insert PlayOpenAnimation before TryShowEquipTutorial
                    lines.Insert(nextLine, "            PlayOpenAnimation();");
                }
                else if (nextLine < lines.Count && lines[nextLine].Contains("PlayOpenAnimation")) {
                    // Already added, skip
                }
                else if (nextLine < lines.Count && lines[nextLine].Trim() == "}") {
                    // ShowForSelection has no TryShowEquipTutorial, add before closing
                    lines.Insert(nextLine, "            PlayOpenAnimation();");
                }
            }
        }

        // 5. Replace CloseAsTab - find it and replace panel.SetActive(false) with PlayCloseAnimation
        for (int i = 0; i < lines.Count; i++) {
            if (lines[i].Contains("public void CloseAsTab()")) {
                // Find and replace "if (panel != null) panel.SetActive(false);" inside this method
                for (int j = i; j < Math.Min(i + 10, lines.Count); j++) {
                    if (lines[j].Contains("panel.SetActive(false)")) {
                        lines[j] = "            PlayCloseAnimation(null);";
                        break;
                    }
                }
                break;
            }
        }

        // 6. Add helper methods before the last two closing braces (class } and namespace })
        // Find the second-to-last "}"
        int lastBrace = -1;
        int secondLastBrace = -1;
        for (int i = lines.Count - 1; i >= 0; i--) {
            if (lines[i].Trim() == "}") {
                if (lastBrace == -1) lastBrace = i;
                else if (secondLastBrace == -1) { secondLastBrace = i; break; }
            }
        }

        if (secondLastBrace > 0) {
            var helpers = new string[] {
                "",
                "        // --- Panel Animation Helpers ---",
                "        private CanvasGroup EnsurePanelCanvasGroup()",
                "        {",
                "            if (panelCanvasGroup != null) return panelCanvasGroup;",
                "            if (panel == null) return null;",
                "            panelCanvasGroup = panel.GetComponent<CanvasGroup>();",
                "            if (panelCanvasGroup == null) panelCanvasGroup = panel.AddComponent<CanvasGroup>();",
                "            panelOriginalScale = panel.transform.localScale;",
                "            if (panelOriginalScale.sqrMagnitude < 0.01f) panelOriginalScale = Vector3.one;",
                "            return panelCanvasGroup;",
                "        }",
                "",
                "        private void PlayOpenAnimation()",
                "        {",
                "            var cg = EnsurePanelCanvasGroup();",
                "            if (cg == null || panel == null) return;",
                "",
                "            cg.alpha = 0f;",
                "            panel.transform.localScale = panelOriginalScale * 0.85f;",
                "",
                "            cg.DOFade(1f, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);",
                "            panel.transform.DOScale(panelOriginalScale, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);",
                "        }",
                "",
                "        private void PlayCloseAnimation(System.Action onComplete)",
                "        {",
                "            var cg = EnsurePanelCanvasGroup();",
                "            if (cg == null || panel == null)",
                "            {",
                "                if (panel != null) panel.SetActive(false);",
                "                onComplete?.Invoke();",
                "                return;",
                "            }",
                "",
                "            isCloseAnimating = true;",
                "            UIInputBlocker_Alpha.Block();",
                "",
                "            cg.DOFade(0f, 0.12f).SetEase(Ease.InCubic).SetUpdate(true);",
                "            panel.transform.DOScale(panelOriginalScale * 0.85f, 0.12f).SetEase(Ease.InCubic).SetUpdate(true)",
                "                .OnComplete(() =>",
                "                {",
                "                    panel.SetActive(false);",
                "                    panel.transform.localScale = panelOriginalScale;",
                "                    cg.alpha = 1f;",
                "                    isCloseAnimating = false;",
                "                    UIInputBlocker_Alpha.Unblock();",
                "                    onComplete?.Invoke();",
                "                });",
                "        }",
            };
            lines.InsertRange(secondLastBrace, helpers);
        }

        File.WriteAllLines(path, lines.ToArray(), new UTF8Encoding(true));
        Console.WriteLine("SUCCESS: InventoryUI_Alpha.cs patched");
    }
}
