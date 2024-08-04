using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public GameObject[] fields;
    public Transform player;

    void Update()
    {
        foreach (var field in fields)
        {
            float distance = Vector3.Distance(player.position, field.transform.position);
            field.SetActive(distance < 10); // 例: 距離が10未満ならアクティブ化
        }
    }
}
