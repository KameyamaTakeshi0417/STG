using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2.0f; // Background scroll speed
    public float resetPositionY = -10.0f; // Position to reset backgrounds
    public float startPositionY = 20.0f; // Start position of backgrounds
    public float appearFinalPositionY = 30f; // Final position of the last background
    public int copseLoopCount = 2; // Number of loops for the copse backgrounds

    public GameObject backgroundA;
    public GameObject backgroundB;
    public GameObject backgroundC;

    public GameObject copseBG_1; // First copse background
    public GameObject copseBG_2; // Second copse background
    public GameObject copseBG_3; // Third copse background
    public GameObject field; // Final field background
    public GameObject player; // Player reference

    private List<GameObject> backgrounds;
    private List<GameObject> copseBackgrounds;
    public bool isCleared = false; // Game cleared state
    private bool isTransitioning = false; // Whether transitioning to the final phase

    void Start()
    {
        backgrounds = new List<GameObject> { backgroundA, backgroundB, backgroundC };
        copseBackgrounds = new List<GameObject> { copseBG_1, copseBG_2, copseBG_3 };

        foreach (var copse in copseBackgrounds)
        {
            copse.SetActive(false);
        }

        field.SetActive(false); // Final field background initially inactive

        StartCoroutine(ScrollBackgrounds());
    }

    IEnumerator ScrollBackgrounds()
    {
        while (!isCleared)
        {
            foreach (GameObject bg in backgrounds)
            {
                Vector3 worldSize = GetObjectWorldSize(bg);

                bg.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

                if (bg.transform.position.y <= resetPositionY - (worldSize.y * 0.5f))
                {
                    GameObject highestBg = GetHighestBackground();
                    bg.transform.position = new Vector3(
                        highestBg.transform.position.x,
                        highestBg.transform.position.y + worldSize.y,
                        highestBg.transform.position.z
                    );
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield return TransitionToCopseBackgrounds();
    }

    Vector3 GetObjectWorldSize(GameObject obj)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        else
        {
            // Default size if no SpriteRenderer exists
            Debug.LogWarning($"SpriteRenderer not found on {obj.name}. Using default size.");
            return new Vector3(10, 10, 0); // Example default size
        }
    }

    GameObject GetHighestBackground()
    {
        GameObject highestBg = backgrounds[0];
        foreach (GameObject bg in backgrounds)
        {
            if (bg.transform.position.y > highestBg.transform.position.y)
            {
                highestBg = bg;
            }
        }
        return highestBg;
    }

    private IEnumerator TransitionToCopseBackgrounds()
    {
        isTransitioning = true;

        foreach (GameObject bg in backgrounds)
        {
            bg.SetActive(false);
        }

        foreach (var copse in copseBackgrounds)
        {
            copse.SetActive(true);
        }

        for (int loop = 0; loop < copseLoopCount; loop++)
        {
            foreach (var copse in copseBackgrounds)
            {
                Vector3 worldSize = GetObjectWorldSize(copse);

                while (copse.transform.position.y > resetPositionY)
                {
                    copse.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
                    yield return null;
                }

                GameObject highestCopse = GetHighestCopse();
                copse.transform.position = new Vector3(
                    highestCopse.transform.position.x,
                    highestCopse.transform.position.y + worldSize.y,
                    highestCopse.transform.position.z
                );
            }
        }

        foreach (var copse in copseBackgrounds)
        {
            copse.SetActive(false);
        }

        field.SetActive(true);

        Vector3 finalPosition = new Vector3(
            field.transform.position.x,
            appearFinalPositionY,
            field.transform.position.z
        );

        while (field.transform.position.y > appearFinalPositionY)
        {
            field.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        field.transform.position = finalPosition;

        isTransitioning = false;
    }

    GameObject GetHighestCopse()
    {
        GameObject highestCopse = copseBackgrounds[0];
        foreach (GameObject copse in copseBackgrounds)
        {
            if (copse.transform.position.y > highestCopse.transform.position.y)
            {
                highestCopse = copse;
            }
        }
        return highestCopse;
    }

    public void SetCleared(bool cleared)
    {
        isCleared = cleared;
    }
}
