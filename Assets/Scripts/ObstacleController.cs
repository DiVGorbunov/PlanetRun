using System;
using System.Collections;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private GameController gameController;

    public GameObject planet;
    public GameObject asteroid;

    private bool isAsteroidInitialized;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    public void Shot()
    {
        Vector3 savedPosition = transform.position;

        StartCoroutine(AnimateSize(gameObject, 0.5f, 0.6f, 0.05f, () => { scaledown(gameObject, savedPosition); }));

    }

    protected void scaledown(GameObject obstacle, Vector3 savedPosition)
    {
        StartCoroutine(AnimateSize(obstacle, 0.6f, 0.1f, 0.25f, () => {

            if (!isAsteroidInitialized)
            {
                planet.SetActive(false);
            }
            else
            {
                asteroid.SetActive(false);
            }

            StartCoroutine(RestoreObstacle(obstacle, savedPosition));
        }));
    }

    protected IEnumerator AnimateSize(GameObject obstacle, float startValue, float endValue, float duration, Action action)
    {
        float elapsedTime = 0;
        float ratio = elapsedTime / duration;
        while (ratio < 1f)
        {
            elapsedTime += Time.deltaTime;
            ratio = elapsedTime / duration;

            float size = startValue + (endValue - startValue) * ratio;

            setSize(obstacle, size);

            yield return null;
        }

        if (action != null)
        {
            action();
        }
    }

    private IEnumerator RestoreObstacle(object obstacleObj, Vector3 position)
    {
        GameObject obstacle = (GameObject)obstacleObj;
        obstacle.transform.position = position;

        yield return new WaitForSeconds(gameController.restoreObstacleDelay);
        if (!isAsteroidInitialized)
        {
            isAsteroidInitialized = true;
        }
        asteroid.SetActive(true);
        setSize(obstacle, 0.5f);
    }

    private void setSize(GameObject obstacle, float size)
    {
        obstacle.transform.localScale = new Vector3(size, size, size);
    }

    public bool IsPlanet()
    {
        return !isAsteroidInitialized;
    }
}
