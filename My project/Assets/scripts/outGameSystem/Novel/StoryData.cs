using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryData
{
   // キャラ.
    public string Name = "";
    // 会話内容.
    [Multiline( 3 )] public string Talk = "";
    // 場所、背景.
    public string Place = "";
    // 左キャラ.
    public string Left = "";
    // 真ん中キャラ.
    public string Center = "";
    // 右キャラ.
    public string Right = "";
}