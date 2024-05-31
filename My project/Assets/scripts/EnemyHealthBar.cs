using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Health enemy; // 参照する敵オブジェクト
    public Image healthBarImage; // HPバーのImageコンポーネント
    public Vector3 offset; // HPバーの位置オフセット
private Camera mainCamera;
void Start(){

      mainCamera = Camera.main;
}
 void Update()
    {
        if (enemy != null)
        {
            if (IsVisibleFrom(enemy.GetComponent<Renderer>(), mainCamera))
            {
                healthBarImage.fillAmount = enemy.getCurrentHP() / enemy.getHP();
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemy.transform.position + offset);
                transform.position = screenPosition;
                healthBarImage.enabled = true;
            }
            else
            {
                healthBarImage.enabled = false;
            }
        }
    }

    private bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}