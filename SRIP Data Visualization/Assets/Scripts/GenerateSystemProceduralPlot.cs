using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class GenerateSystemProceduralPlot : MonoBehaviour
{
    [Header("Axial Plane Properties")]
    [SerializeField] private GameObject axisPlane1;
    [SerializeField] private GameObject axisPlane2;
    [SerializeField] private Material planeMaterial;
    [SerializeField] private int xSize = 30;
    [SerializeField] private int zSize = 30;
    [SerializeField] private Gradient gradient;
    [Header("Equation Properties")]
    [SerializeField] private Slider sliderW0;
    [SerializeField] private Slider sliderD;
    [SerializeField] private Slider sliderK;
    [SerializeField] private TextMeshProUGUI textW0;
    [SerializeField] private TextMeshProUGUI textD;
    [SerializeField] private TextMeshProUGUI textK;

    private Mesh plotMesh;
    private MeshCollider mc;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Color[] colors;
    private float W0;
    private float k;
    private float d;
    private float maxY = 0f;
    private float minY = Mathf.Infinity;

    public float GetEnergyValue(float x, float z)
    {
        return -W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)));
    }

    private void AssignValues()
    {
        W0 = sliderW0.value;
        k = sliderK.value;
        d = sliderD.value;
        textW0.text = sliderW0.value.ToString();
        textK.text = sliderK.value.ToString();
        textD.text = sliderD.value.ToString();
    }

    private void CreatePlot()
    {
        AssignValues();
        vertices = new Vector3[(2 * xSize + 1) * (2 * zSize + 1)];
        for (int i=0, z = -zSize; z <= zSize; z++) 
        {
            for (int x = -xSize; x <= xSize; x++)
            {
                if(maxY < Mathf.Abs(-W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)))))
                {
                    maxY = Mathf.Abs(-W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3))));
                }
                if(minY > Mathf.Abs(-W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)))))
                {
                    minY = Mathf.Abs(-W0 * Mathf.Pow(k, 6) * ((1 / Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3))));
                }
                vertices[i] = new Vector3(x, -W0 * Mathf.Pow(k, 6) * ((1/Mathf.Pow(Mathf.Pow(x - d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3)) + (1 / Mathf.Pow(Mathf.Pow(x + d, 2) + Mathf.Pow(z, 2) + Mathf.Pow(k, 2), 3))), z);
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
                tris+=6;
            }
            vert++;
        }

        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];
        for (int i=0, z=0; z <=2 * zSize; z++)
        {
            for (int x = 0; x <= 2 * xSize; x++)
            {
                uvs[i] = new Vector2(((float)x - (-xSize)) / xSize - (-xSize), ((float)z - (-zSize)) / zSize - (-zSize)); //(float)((x - (-xSize)) / xSize - (-xSize)), (float)((z - (-zSize)) / zSize - (-zSize))
                colors[i] = gradient.Evaluate(Mathf.InverseLerp(minY, maxY, vertices[i].y));
                i++;
            }
        }
    }

    private void UpdateMesh()
    {
        plotMesh.Clear();
        plotMesh.name = "3D Graph";
        plotMesh.vertices = vertices;
        plotMesh.triangles = triangles;
        plotMesh.uv = uvs;
        plotMesh.colors = colors;
        gameObject.tag = "Plot";
        mc.sharedMesh = plotMesh;
        axisPlane1.transform.localScale = new Vector3(xSize * 0.2f, maxY/2, zSize * 0.2f);
        axisPlane2.transform.localScale = new Vector3(xSize * 0.2f, maxY / 2, zSize * 0.2f);
        planeMaterial.SetTextureScale("_MainTex", new Vector2(2, 2));
    }

    // Start is called before the first frame update
    void Start()
    {
        plotMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = plotMesh;
        mc = GetComponent<MeshCollider>();
        CreatePlot();
    }

    // Update is called once per frame
    void Update()
    {
        CreatePlot();
        UpdateMesh();
    }

    private void OnDrawGizmos()
    {
        if(vertices == null)
        {
            return;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
