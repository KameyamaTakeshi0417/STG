using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public GameObject enemyObject; // 参照する敵オブジェクト
    private Health enemyHealthScript;
    public Image healthBarImage; // HPバーのImageコンポーネント
    public Vector3 offset; // HPバーの位置オフセット
    private Camera mainCamera;
void Start(){

      mainCamera = Camera.main;
}
 void Update()
    {
        if (enemyObject != null)
        {
            if (IsVisibleFrom(enemyObject.GetComponent<Renderer>(), mainCamera))
            {
                healthBarImage.fillAmount = enemyHealthScript.getCurrentHP() / enemyHealthScript.getHP();
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemyObject.transform.position + offset);
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
    public void setEnemy(GameObject enemy){
        enemyObject=enemy;
        enemyHealthScript=enemy.GetComponent<Health>();
    }
}