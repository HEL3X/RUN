using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    // Variables exposed in the Unity Editor
    [SerializeField] public float viewRad; // View radius
    [Range(0, 360)]
    [SerializeField] public float viewAng; // View angle
    [SerializeField] LayerMask targetMask; // Layer mask for targets
    [SerializeField] LayerMask obstacleMask; // Layer mask for obstacles

    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>(); // List of visible targets

    [SerializeField] float meshRes; // Mesh resolution
    [SerializeField] int edgeResolveIterations; // Number of iterations for edge resolution
    [SerializeField] float edgeDstThreshold; // Distance threshold for detecting edges

    // References to other components and objects
    public NavMeshAgent agent; 
    [SerializeField] public GameObject player;
    float time; 
    public float delaySearch; 

    [SerializeField] private MeshFilter viewMeshFilter; // Reference to the MeshFilter component for the view mesh
    private Mesh viewMesh; // View mesh

    // Variables for tracking movement and animation
    public bool isSeen = false; 
    public bool isMoving;
    Vector3 oldPos; 
    Vector3 newPos; 
    private Animator anim; 

    // References to other objects and audio sources
    public GameObject onDeath;
    public AudioSource jumpscareSource; 
    public AudioSource audioSource1; 

    public hatch h; 

    private void Start()
    {
        // Initialize view mesh
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        // Initialize variables and references
        anim = GetComponent<Animator>();
        onDeath.SetActive(false);
        Time.timeScale = 1;

        // Start searching for targets with a delay
        StartCoroutine(FindTargetsWithDelay(.2f));
    }

    public void Update()
    {
        // Update timer and player position
        time += Time.deltaTime;
        Vector3 vector3 = player.transform.position;
        checkStanding();

        // Adjust delay based on whether the player is seen or not
        if (isSeen)
        {
            delaySearch = 0.5f;
        }
        else
        {
            delaySearch = 3f;
        }

        // Search for targets after the delay
        if (time > delaySearch)
        {
            agent.SetDestination(vector3);
            time = 0;
        }

        // Stop the audio source if the player has won the game
        if (h.hasWon)
        {
            audioSource1.Stop();
        }
    }

    void checkStanding()
    {
        // Check if the character is standing still or moving
        newPos = transform.position;
        if (newPos != oldPos)
        {
            isMoving = true;
            anim.SetBool("isWalking", true);
        }
        else
        {
            isMoving = false;
            anim.SetBool("isWalking", false);
        }
        oldPos = newPos;
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        isSeen = false;

        // Find all colliders within the view radius
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRad, targetMask);

        // Check if each target is within the view angle and not obstructed by obstacles
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAng / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                    isSeen = true;
                }
            }
        }
    }

    private void DrawFieldOfView()
    {
        // Calculate the number of steps and angle size
        int stepCount = Mathf.RoundToInt(viewAng * meshRes);
        float stepAngSize = viewAng / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        // Cast rays to detect obstacles and find view points
        for (int i = 0; i <= stepCount; i++)
        {
            float ang = transform.eulerAngles.y - viewAng / 2 + stepAngSize * i;
            ViewCastInfo newViewCast = ViewCast(ang);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // Create the view mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAng = minViewCast.angle;
        float maxAng = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxP = Vector3.zero;

        // Perform binary search to find the edge between obstructed and unobstructed view
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float ang = (minAng + maxAng) / 2;
            ViewCastInfo newViewCast = ViewCast(ang);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAng = ang;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAng = ang;
                maxP = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxP);
    }

    private ViewCastInfo ViewCast(float globalAng)
    {
        // Cast a ray in the specified direction and check for obstacles
        Vector3 dir = DirFromAng(globalAng, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRad, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAng);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRad, viewRad, globalAng);
        }
    }

    public Vector3 DirFromAng(float ang, bool anglGlobal)
    {
        // Calculate the direction vector from the angle
        if (!anglGlobal)
        {
            ang += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad), 0, Mathf.Cos(ang * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        // Struct to hold information about a casted ray
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float dst, float ang)
        {
            this.hit = hit;
            this.point = point;
            this.dst = dst;
            this.angle = ang;
        }
    }

    public struct EdgeInfo
    {
        // Struct to hold information about an edge point
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle collision with the player
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("You Lost");
            audioSource1.Stop();
            jumpscareSource.Play();
            onDeath.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
