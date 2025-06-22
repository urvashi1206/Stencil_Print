using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCBSpawner : MonoBehaviour
{
    public GameObject pcbPrefab;
    public Transform spawnPoint;
    public StencilControl stencilControl;

    private bool canSpawn = true;

    // Start is called before the first frame update 
    void Start()
    {
        StartCoroutine(SpawnPCBCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnPCBCoroutine()
    {
        while (canSpawn)
        {
            canSpawn = false;
            GameObject pcbInstance = Instantiate(pcbPrefab, spawnPoint.position, pcbPrefab.transform.rotation);
            stencilControl.SetPCB(pcbInstance.transform);
            stencilControl.SetSpawner(this);
            yield return null; // Wait for the next frame
        }
    }

    public void NotifyPCBCompleted()
    {
        StartCoroutine(WaitAndSpawnNext());
    }

    IEnumerator WaitAndSpawnNext()
    {
        yield return new WaitForSeconds(1f);
        canSpawn = true;
        StartCoroutine(SpawnPCBCoroutine());
    }
}
