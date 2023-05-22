using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class FOV : MonoBehaviour
{
    [SerializeField] public float viewRad;
    [Range(0, 360)]
    [SerializeField] public float viewAng; 
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;

    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    [SerializeField] float meshRes;
    [SerializeField] int edgeResolveIterations;
    [SerializeField] float edgeDstThreshold;

    public NavMeshAgent agent;
    [SerializeField] public GameObject player;
    float time;
    public float delaySearch;

    [SerializeField] private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    public bool isSeen = false;

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine(FindTargetsWithDelay(.2f));
    }

    public void Update()
    {
        time += Time.deltaTime;
        Vector3 vector3 = player.transform.position;

        if (isSeen)
        {
            delaySearch = 0.5f;
        }
        else
        {
            delaySearch = 3f;
        }

        if (time > delaySearch)
        {
            agent.SetDestination(vector3);
            time = 0;
        }
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

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRad, targetMask);

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
        int stepCount = Mathf.RoundToInt(viewAng * meshRes);
        float stepAngSize = viewAng / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
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
        if (!anglGlobal)
        {
            ang += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad), 0, Mathf.Cos(ang * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
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
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }
}