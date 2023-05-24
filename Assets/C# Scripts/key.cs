using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class key : MonoBehaviour
{
    [SerializeField] public Vector3 rot;
    [SerializeField] public float speed;
    private int count = 0;

    [SerializeField] public hatch h;

    public void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rot * speed * Time.deltaTime);   
    }

    void OnTriggerEnter(Collider trigger)
    {
        if (trigger.gameObject.tag == "Player")
        {
            Debug.Log("You Collected A Key");
            h.hasKey = true;
            Destroy(this.gameObject);
        }
    }
}
