using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatch : MonoBehaviour
{

    public bool hasKey = false;
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" && hasKey)
        {
            Debug.Log("You Won!");
        }
        else if (col.gameObject.tag == "Player")
        {
            Debug.Log("Find the key!");
        }
    }
}
