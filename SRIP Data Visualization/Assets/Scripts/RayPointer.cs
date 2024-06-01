using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayPointer : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftController;
    [SerializeField] private XRRayInteractor rightController;
    [SerializeField] private LayerMask plotLayer;
    [SerializeField] private TextMeshPro text;

    private float energyValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform.position, Vector3.up);
        if (leftController.TryGetCurrent3DRaycastHit(out var raycastHit) || rightController.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            if (raycastHit.collider.CompareTag("Plot"))
            {
                transform.position = raycastHit.point;
                Debug.Log("Local : " + transform.localPosition.x + "\nGlobal : " + transform.position.x);
                energyValue = raycastHit.collider.GetComponent<GenerateSystemProceduralPlot>().GetEnergyValue(transform.localPosition.x, transform.localPosition.z);
                text.text = energyValue.ToString();
            }
        }
    }
}
