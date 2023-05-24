using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatch : MonoBehaviour
{

    public bool hasKey = false;

    void OnTriggerEnter(Collider trigger)
    {
        Debug.Log("" + trigger.gameObject.tag);
        if (trigger.gameObject.tag == "Player" && hasKey)
        {
            Debug.Log("You Won!");
        }
        else if (trigger.gameObject.tag == "Player")
        {
            Debug.Log("Find the key!");
        }
    }
}
