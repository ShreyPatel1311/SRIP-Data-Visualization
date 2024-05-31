using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class GenerateProceduralPlot : MonoBehaviour
{
    [SerializeField] private int xSize = 30;
    [SerializeField] private int zSize = 30;
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
    private float W0;
    private float k;
    private float d;

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

    }

    private void UpdateMesh()
    {
        plotMesh.Clear();
        plotMesh.name = "3D Graph";
        plotMesh.vertices = vertices;
        plotMesh.triangles = triangles;
        plotMesh.RecalculateNormals();
        gameObject.tag = "Plot";
        mc.sharedMesh = plotMesh;
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
