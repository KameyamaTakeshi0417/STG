using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string scriptPath = @"C:\Users\kanin\Documents\STG\new_script.txt";
        string assetPath = @"C:\Users\kanin\Documents\STG\My project\Assets\scripts\Alpha\UI\ADV\stage1\stage1AfterBoss.asset";
        
        string[] lines = File.ReadAllLines(scriptPath, Encoding.UTF8);
        
        var pages = new List<string>();
        
        string currentSpeaker = "";
        string currentText = "";
        
        Action commitPage = () => {
            if (string.IsNullOrWhiteSpace(currentSpeaker) && string.IsNullOrWhiteSpace(currentText)) return;
            
            string spk = currentSpeaker.Trim();
            
            int leftSpeaking = 0;
            int rightSpeaking = 0;
            int centerSpeaking = 0;
            
            // REVERSED AS PER USER REQUEST!
            if (spk.Contains("ヨウ") && spk.Contains("カイカ")) {
                leftSpeaking = 1;
                rightSpeaking = 1;
            } else if (spk.Contains("ヨウ")) {
                rightSpeaking = 1; // Used to be leftSpeaking = 1
            } else if (spk.Contains("カイカ")) {
                leftSpeaking = 1; // Used to be rightSpeaking = 1
            } else if (spk.Contains("ツバキ") || spk.Contains("レン") || spk.Contains("商人") || spk.Contains("ミノムシ")) {
                centerSpeaking = 1;
            }
            
            string leftChar = "{fileID: 21300000, guid: 7c29803e5f6f0154ab467ba1fa964ca8, type: 3}";
            string rightChar = "{fileID: 21300000, guid: 0287fe58e23102f42b6e86ecf8d5fa43, type: 3}";
            string centerChar = "{fileID: 21300000, guid: 51f28dc9d2d04d1478e690004776fe96, type: 3}";
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  - characterName: " + (spk == "" ? "" : "\"" + spk + "\""));
            
            string t = currentText.TrimEnd().Replace("\r", "").Replace("\n", "\\n").Replace("\"", "\\\"");
            sb.AppendLine("    dialogueText: \"" + t + "\"");
            
            sb.AppendLine("    leftCharacter: " + leftChar);
            sb.AppendLine("    rightCharacter: " + rightChar);
            sb.AppendLine("    centerCharacter: " + centerChar);
            sb.AppendLine("    leftCharacterAnim: 0");
            sb.AppendLine("    centerCharacterAnim: 0");
            sb.AppendLine("    rightCharacterAnim: 0");
            sb.AppendLine("    waitForAnimationToFinish: 0");
            sb.AppendLine("    backgroundImage: {fileID: 21300000, guid: 51429e7ddc535764790765a12e711d17, type: 3}");
            sb.AppendLine("    eventCG: {fileID: 0}");
            sb.AppendLine("    bgmClip: {fileID: 0}");
            sb.AppendLine("    seClip: {fileID: 0}");
            sb.AppendLine("    leftSpeaking: " + leftSpeaking);
            sb.AppendLine("    centerSpeaking: " + centerSpeaking);
            sb.AppendLine("    rightSpeaking: " + rightSpeaking);
            
            pages.Add(sb.ToString().TrimEnd());
            
            currentSpeaker = "";
            currentText = "";
        };

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var m1 = Regex.Match(line, @"^【(.+?)】\s*(.*)$");
            var m2 = Regex.Match(line, @"^([^\s　]+)[　\s]+(.*)$");
            
            if (m1.Success) {
                commitPage();
                currentSpeaker = m1.Groups[1].Value;
                currentText = m1.Groups[2].Value + "\n";
            } else if (m2.Success && !line.StartsWith(" ") && !line.StartsWith("　")) {
                string possibleSpeaker = m2.Groups[1].Value;
                if (possibleSpeaker == "ヨウ" || possibleSpeaker == "カイカ" || possibleSpeaker == "ツバキ" || possibleSpeaker == "ミノムシ？" || possibleSpeaker.Contains("商人") || possibleSpeaker.Contains("レン") || possibleSpeaker == "カイカとヨウ" || possibleSpeaker == "ヨウとカイカ") {
                    commitPage();
                    currentSpeaker = possibleSpeaker;
                    currentText = m2.Groups[2].Value + "\n";
                } else {
                    currentText += line.Trim() + "\n";
                }
            } else {
                currentText += line.Trim() + "\n";
            }
        }
        commitPage();
        
        for (int i = 0; i < 8 && i < pages.Count; i++) {
            pages[i] = pages[i].Replace("centerCharacter: {fileID: 21300000, guid: 51f28dc9d2d04d1478e690004776fe96, type: 3}", "centerCharacter: {fileID: 0}");
        }

        string originalAsset = File.ReadAllText(assetPath, Encoding.UTF8);
        int pagesIndex = originalAsset.IndexOf("  pages:");
        if (pagesIndex == -1) {
            Console.WriteLine("Could not find '  pages:' in asset.");
            return;
        }
        
        string header = originalAsset.Substring(0, pagesIndex + "  pages:\n".Length);
        
        using (StreamWriter sw = new StreamWriter(assetPath, false, new UTF8Encoding(false))) {
            sw.Write(header);
            foreach(var p in pages) {
                sw.WriteLine(p);
            }
        }
        
        Console.WriteLine("Done rewriting asset with REVERSED speaking flags. Generated " + pages.Count + " pages.");
    }
}
