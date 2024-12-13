using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public GameObject hexPrefab; // 六角形のプレハブをアサイン
    public int width = 3; // 横の数
    public int height = 9; // 縦の数
    public float hexWidth = 1.0f; // 六角形の幅
    public float hexHeight = 1.0f; // 六角形の高さ

    void Start()
    {
        GameObject hexPrefab_StartFloor = Resources.Load<GameObject>("Objects/StartFloor");
        Instantiate(hexPrefab_StartFloor, new Vector3(0, 0, 0), Quaternion.identity);
        CreateHexGrid();
    }

    void CreateHexGrid()
    {
        // 六角形の隣接距離を計算
        float xOffset = (hexWidth * 0.75f); // 幅の3/4
        float yOffset = (hexHeight * 0.866f); // 高さの√3/2 ≈ 0.866

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 基本の位置を計算
                float xPos = x * xOffset;
                float yPos = y * yOffset;

                // 奇数行の場合、yPosをオフセット
                if (x % 2 == 1)
                {
                    yPos += yOffset * 0.5f;
                }

                Vector3 pos = new Vector3(xPos - xOffset, yPos + (hexHeight * 0.45f), 0);
                Instantiate(hexPrefab, pos, Quaternion.identity);
            }
        }
    }
}
