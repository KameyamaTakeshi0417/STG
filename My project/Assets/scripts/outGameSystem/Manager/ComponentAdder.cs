using UnityEngine;

public class ComponentAdder : MonoBehaviour
{
    // 任意のスクリプト名を指定してAddComponentを実行
    public void AddCaseByName(string scriptName, GameObject targetObj)
    {
        // スクリプト名からTypeを取得
        System.Type componentType = System.Type.GetType(scriptName);

        if (componentType != null && typeof(Case_Base).IsAssignableFrom(componentType))
        {
            targetObj.AddComponent(componentType);
            Debug.Log("Component added: " + componentType.Name);
        }
        else
        {
            Debug.LogWarning(
                "The component type is not valid or does not inherit from BaseScript."
            );
        }
    }
}
