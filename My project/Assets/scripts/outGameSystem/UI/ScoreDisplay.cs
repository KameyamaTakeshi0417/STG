using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text scoreText;

    private void Update()
    {
        scoreText.text = "Score: " + GameManager.Instance.score.ToString();
    }
}
