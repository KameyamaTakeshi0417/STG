using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureManager_Alpha : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private List<int> stackDisplayList = new List<int>();
    public Stack<int> treasure=new Stack<int>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetTreasure(int rarelity) {
    treasure.Push(rarelity);
        SyncToDisplay();
    }
    public void PopItem()
    {
        if (treasure.Count > 0)
        {
            treasure.Pop();
            SyncToDisplay();
        }
    }
    private void SyncToDisplay()
    {
        stackDisplayList.Clear();
        foreach (var item in treasure)
        {
            stackDisplayList.Add(item);
        }
    }

}
