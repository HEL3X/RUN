using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatch : MonoBehaviour
{

    public bool hasKey = false;
    public GameObject onWin;

    public void Start()
    {
        onWin.SetActive(false);
    }


    void OnTriggerEnter(Collider trigger)
    {
        Debug.Log("" + trigger.gameObject.tag);
        if (trigger.gameObject.tag == "Player" && hasKey)
        {
            Debug.Log("You Won!");
            onWin.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (trigger.gameObject.tag == "Player")
        {
            Debug.Log("Find the key!");
        }
    }
}
