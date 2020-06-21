using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public delegate void del();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyObstacle()
    {
        StartCoroutine(AnimateSize(0.5f,0.1f,0.5f,null));
    }

    public IEnumerator CanPortal()
    {
        yield return new WaitForSeconds(0.5f);
    }

    protected IEnumerator AnimateSize(float startValue, float endValue, float duration, del action)
    {
        float elapsedTime = 0;
        float ratio = elapsedTime / duration;
        while (ratio < 1f)
        {
            elapsedTime += Time.deltaTime;
            ratio = elapsedTime / duration;

            float size = startValue + (endValue - startValue) * ratio;

            setSize(size);

            yield return null;
        }

        if (action != null)
        {
            action();
        }
    }

    private void setSize(float size)
    {
        gameObject.transform.localScale = new Vector3(size, size, size);
    }
}
