using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Instantiator : MonoBehaviour
{
    [SerializeField] private GameObject plotPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreatePlot()
    {
        GameObject newPlot = Instantiate(plotPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        newPlot.AddComponent<XRGrabInteractable>();
        newPlot.GetComponent<Rigidbody>().useGravity = false;
        newPlot.GetComponent <Rigidbody>().isKinematic = true;
    }
}
