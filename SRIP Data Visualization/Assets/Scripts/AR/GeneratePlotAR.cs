using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeneratePlotAR : MonoBehaviour
{
    [Header("Plot Dimensions")]
    [SerializeField] private int xSize;
    [SerializeField] private int zSize;
    [SerializeField] private GameObject pointer;
    [SerializeField] private GameObject gridPoint;
    [SerializeField] private TextMeshPro pointerText;

    [Header("Plot Equation Properties")]
    [SerializeField] private float W0;
    [SerializeField] private float k;
    [SerializeField] private float d;
    [SerializeField] private float thresholdDistance;

    [Header("Weave Plot Dimensions")]
    [SerializeField] private GameObject weavePlot;
    [SerializeField] private GameObject weavePointer;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private MeshCollider mc;
    private List<GameObject> gridPoints = new List<GameObject>();
    private TouchControls touchControls;

    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;

    private Mesh plotMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Mesh plotWeaveMesh;
    private Vector3[] weaveVertices;
    private int[] weaveTriangles;
    private Vector2[] weaveUvs;
    private Vector3 startPointerPosition;
    private Ray ray;

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

    //Do Something when plot is touched
    private void StartTouch(InputAction.CallbackContext ctx)
    {
        Vector2 touchPos = touchControls.Touch.TouchPosition.ReadValue<Vector2>();
        Ray rayFromTouchPos = Camera.main.ScreenPointToRay(touchPos);
        ray = rayFromTouchPos;
        if (Physics.Raycast(rayFromTouchPos, out var raycastHit, 100f))
        {
            int i = 0;
            weavePointer.transform.position = raycastHit.point;
            foreach (GameObject point in gridPoints)
            {
                if (Vector3.Distance(point.transform.position, weavePointer.transform.position) <= thresholdDistance)
                {
                    vertices[i].y = U(vertices[i].x, vertices[i].z);
                    Debug.Log(vertices[i].x + " , " + vertices[i].z);
                    pointer.transform.position = startPointerPosition + vertices[i];
                    pointerText.text = U(vertices[i].x, vertices[i].z).ToString();
                }
                i++;
            }
        }
        if (OnStartTouch != null)
        {
            OnStartTouch(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float)ctx.startTime);
        }
    }

    private void Awake()
    {
        touchControls = new TouchControls();
    }

    private void OnEnable()
    {
        touchControls.Enable();
    }

    private void OnDisable()
    {
        touchControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<ARAnchor>();
        weavePlot.AddComponent<ARAnchor>();

        plotMesh = new Mesh();
        plotWeaveMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = plotMesh;
        weavePlot.GetComponent<MeshFilter>().mesh = plotWeaveMesh;

        CreatePlot(out vertices, out triangles, out uvs);
        CreatePlot(out weaveVertices, out weaveTriangles, out weaveUvs);

        mc = GetComponent<MeshCollider>();
        mc.sharedMesh = plotMesh;
        weavePlot.GetComponent<MeshCollider>().sharedMesh = plotWeaveMesh;

        for (int z = -zSize; z <= zSize; z++)
        {
            for (int x = -xSize; x <= xSize; x++)
            {
                GameObject point = Instantiate(gridPoint, weavePlot.transform.position + (new Vector3(x, 0, z) * 0.5f), Quaternion.identity, weavePlot.transform);
                gridPoints.Add(point);
            }
        }

        startPointerPosition = pointer.transform.position;
        touchControls.Touch.TouchPress.started += ctx => StartTouch(ctx);
    }

    // Update is called once per frame
    void Update()
    {
        pointerText.transform.parent.LookAt(Camera.main.transform.position);
        UpdateMesh(plotWeaveMesh, weaveVertices, weaveTriangles, weaveUvs);
        UpdateMesh(plotMesh, vertices, triangles, uvs);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction.normalized * 100f);
    }
}
