using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager_Alpha : MonoBehaviour
{
    public static SceneManager_Alpha Instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {

        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // インゲームマネージャーはシーン破棄と共に消えるべきなので削除
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
