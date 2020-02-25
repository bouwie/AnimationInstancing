using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceTest : MonoBehaviour
{
    [SerializeField] private int units = 100;
    [SerializeField] private float space = 1;
    [SerializeField] private GameObject animatorUnitPrefab;
    [SerializeField] private GameObject aiUnitPrefab;
    [SerializeField] private Text unitText;
    [SerializeField] private Text fpsText;


    private float fps;
    private float avgFps;

    private void Start() {
        unitText.text = "units: " + transform.childCount;
    }

    public void SpawnAI() {
        StopAllCoroutines();
        StartCoroutine(Spawn(aiUnitPrefab));
    }

    public void SpawnAnimator() {
        StopAllCoroutines();
        StartCoroutine(Spawn(animatorUnitPrefab));
    }

    private void Update() {
        fps = (int)(1f / Time.unscaledDeltaTime);

        avgFps += ((Time.deltaTime / Time.timeScale) - avgFps) * 0.03f;

        float avgDisplayValue = (1F / avgFps);

        fpsText.text = "fps:" + fps + "\n" + "avg fps:" + avgDisplayValue;
    }


    IEnumerator Spawn(GameObject prefab)
    {
        foreach(Transform child in transform) {
            GameObject.Destroy(child.gameObject);
            unitText.text = "units: " + transform.childCount;
        }
        avgFps = 0;

        int unitsSqrt = (int)Mathf.Sqrt(units);
            for(int y = 0; y < unitsSqrt; y++)
        {
            for (int x = 0; x < unitsSqrt; x++)
            {
                Vector3 localPos = new Vector3(x * space, 0, -y * space);
                GameObject newObj = Instantiate(prefab, transform);
                newObj.transform.localPosition = localPos;
                unitText.text = "units: " + transform.childCount;
            }
            yield return new WaitForEndOfFrame();
            }
        
    }
}
