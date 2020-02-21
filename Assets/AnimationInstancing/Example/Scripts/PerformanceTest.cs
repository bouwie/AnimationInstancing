using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    [SerializeField] private int units = 100;
    [SerializeField] private float space = 1;
    [SerializeField] private GameObject animatorUnitPrefab;
    [SerializeField] private GameObject aiUnitPrefab;

    void Start()
    {
        StartCoroutine(Spawn(aiUnitPrefab));
    }

    //IEnumerator Delete()
    //{
    //    for(int i = 0; i < )
    //}

    IEnumerator Spawn(GameObject prefab)
    {
        int unitsSqrt = (int)Mathf.Sqrt(units);
            for(int y = 0; y < unitsSqrt; y++)
        {
            for (int x = 0; x < unitsSqrt; x++)
            {
                Vector3 localPos = new Vector3(x * space, 0, -y * space);
                GameObject newObj = Instantiate(prefab, transform);
                newObj.transform.localPosition = localPos;
            }
            yield return new WaitForEndOfFrame();
            }
        
    }
}
