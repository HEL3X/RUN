using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    public NavMeshAgent agent;
    [SerializeField] public GameObject player;
    float time;
    // Update is called once per frame
    private void Start()
    {
        
    }
    void Update()
    {
        time += Time.deltaTime;
        Vector3 vector3 = player.transform.position;
        if (time > 0.5)
        {
            agent.SetDestination(vector3);
            time = 0;
        }
 
    }
}
