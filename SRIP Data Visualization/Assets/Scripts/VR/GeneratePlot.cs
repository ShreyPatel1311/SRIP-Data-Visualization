using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshFilter))]
public class GeneratePlot : MonoBehaviour
{
    [SerializeField] private int xSize;
    [SerializeField] private int zSize;
    [SerializeField] private float W0;
    [SerializeField] private float k;
    [SerializeField] private float d;
    [SerializeField] private GameObject plot;
    [SerializeField] private GameObject weavePointer;
    [SerializeField] private TextMeshPro pointerValue;
    [SerializeField] private GameObject pointer;
    [SerializeField] private float thresholdDistance;
    [SerializeField] private XRRayInteractor leftController;
    [SerializeField] protected XRRayInteractor rightController;

    private MeshCollider mc;
    private Vector3 startWeavePosition;

    private Mesh plotWeaveMesh;
    private Vector3[] weaveVertices;
    private GameObject player;
    private int[] weaveTriangles;
    private Vector2[] weaveUvs;

    private Mesh plotMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    // used to create initial plot
    public void CreatePlot(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
    {
        vertices = new Vector3[(2 * xSize + 1) * (2 * zSize + 1)];
        for (int i = 0, z = -zSize; z <= zSize; z++)
        {
            for (int x = -xSize; x <= xSize; x++)
            {
                
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6 * 4];

        int vert = 0;
        int tris = 0;
        for (int z = -zSize; z < zSize; z++)
        {
            for (int x = -xSize; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + 1 + 2 * xSize;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + 1 + 2 * xSize;
                triangles[tris + 5] = vert + 2 + 2 * xSize;

                vert++;
                tris += 6;
            }
            vert++;
        }

        uvs = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= 2 * zSize; z++)
        {
            for (int x = 0; x <= 2 * xSize; x++)
            {
                uvs[i] = new Vector2(((float)x - (-xSize)) / xSize - (-xSize), ((float)z - (-zSize)) / zSize - (-zSize)); 
                i++;
            }
        }
    }

    // To Update the mesh values at each frame
    private void UpdateMesh(Mesh mesh, Vector3[] vertices, int[] triangles, Vector2[] uvs)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    //will be used to get the energy value.
    public float U(float x, float z)
    {
        return -W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)));
    }

    //Gets the nearest vertex of the mesh w.r.t weavePointer
    private void DetectVertex()
    {
        for (int v = 0; v < vertices.Length; v++)
        {
            if (Vector3.Distance(vertices[v] * transform.localScale.x, weavePointer.transform.position - startWeavePosition) <= thresholdDistance)
            {
                vertices[v].y = U(vertices[v].x, vertices[v].z);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        plotWeaveMesh = new Mesh();
        startWeavePosition = weavePointer.transform.position;
        plotMesh = new Mesh();
        player = GameObject.Find("Player");
        GetComponent<MeshFilter>().mesh = plotWeaveMesh;
        mc = GetComponent<MeshCollider>();
        plot.GetComponent<MeshFilter>().mesh = plotMesh;
        CreatePlot(out vertices, out triangles, out uvs);
        CreatePlot(out weaveVertices, out weaveTriangles, out weaveUvs);
    }

    // Update is called once per frame
    void Update()
    {
        pointer.transform.LookAt(player.transform.position);
        UpdateMesh(plotMesh, vertices, triangles, uvs);
        UpdateMesh(plotWeaveMesh, weaveVertices, weaveTriangles, weaveUvs);
        mc.sharedMesh = plotWeaveMesh;
        if(leftController.TryGetCurrent3DRaycastHit(out var raycastHit))
        {
            if (raycastHit.collider.CompareTag("Plot"))
            {
                weavePointer.transform.position = raycastHit.point;
                Vector3 changeInPosition = weavePointer.transform.localPosition - raycastHit.point;
                pointerValue.text = U(changeInPosition.x, changeInPosition.z).ToString();
                pointer.transform.localPosition = new Vector3(changeInPosition.x, U(changeInPosition.x, changeInPosition.z), changeInPosition.z);
                DetectVertex();
                UpdateMesh(plotMesh, vertices, triangles, uvs);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
