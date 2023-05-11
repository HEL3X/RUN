using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] public GameObject mazePrefab;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i+=5)
        {
            for (int j = 0; j < 100; j+=5)
            {
                Vector3 pos = new Vector3(i, 3, j);
                if (pos != new Vector3(0, 3, 0) && pos != new Vector3(95, 3, 95))
                {
                    Instantiate(mazePrefab, pos, Quaternion.identity);
                }
                
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
