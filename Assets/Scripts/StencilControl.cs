using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Obi.Samples;
using Obi;
using System.Net;

public class StencilControl : MonoBehaviour
{
    [Header("Machine Parts")]
    public Transform squeegeeA;
    public Transform squeegeeB;
    //public Transform[] pasteAreas;

    //[Header("Paste Prefab")]
    //[Tooltip("Prefab whose children are the 6 paste‑layers (initially disabled).")]
    //public GameObject solderPastePrefab;

    [Header("PCB References")]
    public Transform pcb;
    public Transform pcbSpawnPoint;
    public Transform machine1EntryPoint;
    public Transform machine1WaitPoint;
    public Transform machine1ExitPoint;
    public Transform machine2EntryPoint;
    public Transform machine2WaitPoint;
    public Transform machine2ExitPoint;

    [Header("Movement Settings")]
    public float pcbMoveSpeed;
    public float squeegeeSpeed;
    public float moveDistance;
    public float waitAtMachine2;
    public float destroyAfterSeconds;

    [Header("Stencil Settings")]
    [Tooltip("Y local‑pos when blade is down on stencil")]
    public float bladePrintY;

    [Header("OBI Solver Parameters")]
    public GameObject solverObject;
    public ObiEmitter solderEmitter1;
    public ObiEmitter solderEmitter2;

    private Vector3 pcbTargetPosition;
    private bool moving = true;
    private bool nextUseBladeA = true;

    private Vector3 startA, startB;

    private PCBSpawner spawner;
    void Start()
    {
        startA = squeegeeA.localPosition;
        startB = squeegeeB.localPosition;

    }

    public void SetSpawner(PCBSpawner spawner)
    {
        this.spawner = spawner;
    }

    void LogFixedCheck(string tag)
    {
        if (!Time.inFixedTimeStep)
            Debug.LogWarning($"{tag}  inFixed=false  (frame {Time.frameCount})\n{Environment.StackTrace}");
    }

    public void SetPCB(Transform pcbTransform)
    {
        pcb = pcbTransform;

        Renderer[] pcbRenderers = pcb.GetComponentsInChildren<Renderer>();
        if (pcbRenderers.Length == 0)
        {
            Debug.LogWarning("PCB has no Renderer!");
            return;
        }
        Bounds pcbBounds = pcbRenderers[0].bounds;
        for (int i = 1; i < pcbRenderers.Length; i++)
            pcbBounds.Encapsulate(pcbRenderers[i].bounds);

        Vector3 pcbCenter = pcbBounds.center;
        float pcbTopY = pcbBounds.max.y;

        //GameObject pasteInstance = Instantiate(solderPastePrefab);

        //Renderer pasteRend = pasteInstance.GetComponentInChildren<Renderer>();
        //float pasteHalfHeight = pasteRend.bounds.size.y * 100f;

        //Vector3 pastePos = new Vector3(
        //    pcbCenter.x,
        //    pcbTopY + pasteHalfHeight,
        //    pcbCenter.z
        //);
        //pasteInstance.transform.position = pastePos;

        //var rend = pasteInstance.GetComponent<Renderer>();
        //if (rend != null) rend.enabled = false;

        //pasteInstance.transform.SetParent(pcb, true);

        //pasteAreas = new Transform[] { pasteInstance.transform };

        StartCoroutine(FullPCBProcess());
    }

    IEnumerator FullPCBProcess()
    {
        //Move from spawn to Machine 1 entry
        yield return StartCoroutine(MovePCBToPosition(machine1EntryPoint.position));
        yield return new WaitForSeconds(1f);

        //Wait at machine 2
        yield return StartCoroutine(MovePCBToPosition(machine1WaitPoint.position));
        yield return new WaitForSeconds(1f);

        //Move PCB up to stencil
        pcbTargetPosition = new Vector3(pcb.position.x, transform.position.y + 190f, pcb.position.z);
        yield return new WaitForFixedUpdate();
        while (Vector3.Distance(pcb.position, pcbTargetPosition) > 0.007f)
        {
            LogFixedCheck("MovePCBToPosition loop");
            pcb.position = Vector3.MoveTowards(pcb.position, pcbTargetPosition, pcbMoveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(1f);

        //Print PCB
        yield return StartCoroutine(StencilProcess());
        yield return new WaitForSeconds(1f);

        //Move PCB down
        yield return StartCoroutine(MovePCBToPosition(machine1WaitPoint.position));
        yield return new WaitForSeconds(1f);

        //Move out of Machine 1
        yield return StartCoroutine(MovePCBToPosition(machine1ExitPoint.position));
        yield return new WaitForSeconds(1f);

        //Move into Machine 2
        yield return StartCoroutine(MovePCBToPosition(machine2EntryPoint.position));
        yield return new WaitForSeconds(1f);

        //Wait into Machine 2
        yield return StartCoroutine(MovePCBToPosition(machine2WaitPoint.position));
        yield return new WaitForSeconds(3f);

        //Move out of Machine 2
        yield return StartCoroutine(MovePCBToPosition(machine2ExitPoint.position));
        yield return new WaitForSeconds(1f);

        //Wait and destroy PCB
        yield return new WaitForSeconds(destroyAfterSeconds);
        Debug.Log("Notifying PCB completion to spawner.");
        if (pcb != null)
        {
            solverObject.GetComponent<SolidifyOnContact>().ClearSolids();
            spawner.NotifyPCBCompleted();
            Destroy(pcb.gameObject);
        }

    }

    IEnumerator MovePCBToPosition(Vector3 targetPosition)
    {
        if (pcb == null)
            yield break;

        yield return new WaitForFixedUpdate();

        //Debug.Log("Moving PCB to: " + targetPosition);
        while (Vector3.Distance(pcb.position, targetPosition) > 0.01f)
        {
            LogFixedCheck("MovePCBToPosition loop");
            pcb.position = Vector3.MoveTowards(pcb.position, targetPosition, pcbMoveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
    }

    IEnumerator StencilProcess()
    {
        bool useBladeA = nextUseBladeA;
        Debug.Log(useBladeA);
        Transform active = nextUseBladeA ? squeegeeA : squeegeeB;

        if (useBladeA)
        {
            if (solderEmitter2.enabled)
                solderEmitter2.KillAll();
            solderEmitter2.enabled = false;

            solderEmitter1.KillAll();         
            solderEmitter1.enabled = true;
        }
        else
        {
            if (solderEmitter1.enabled)
                solderEmitter1.KillAll();
            solderEmitter1.enabled = false;

            solderEmitter2.KillAll();          
            solderEmitter2.enabled = true;
        }

        nextUseBladeA = !nextUseBladeA;
        bool forward = moving;
        moving = !moving;

        float origY = active.localPosition.y;

        yield return StartCoroutine(AnimateBladeY(active, origY, bladePrintY));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SlideBlades(forward));
        yield return new WaitForSeconds(1f);

        //if (pasteAreas != null
        //    && pasteAreas.Length > 0)
        //{
        //    var rend = pasteAreas[0].GetComponent<Renderer>();
        //    if (rend != null)
        //        rend.enabled = true;
        //}

        yield return StartCoroutine(AnimateBladeY(active, bladePrintY, origY));
        //moving = !moving;

        yield return new WaitForSeconds(1f);
    }

    IEnumerator AnimateBladeY(Transform blade, float fromY, float toY)
    {
        Vector3 start = blade.localPosition;
        Vector3 end = new Vector3(start.x, toY, start.z);
        while (Mathf.Abs(blade.localPosition.y - toY) > 0.001f)
        {
            blade.localPosition = Vector3.MoveTowards(
                blade.localPosition,
                end,
                squeegeeSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    IEnumerator SlideBlades(bool forward)
    {
        //Debug.Log("Sliding blades: " + forward);
        Vector3 targetA = new Vector3(
            startA.x,
            squeegeeA.localPosition.y,
            forward
                ? startA.z + moveDistance
                : startA.z
        );
        Vector3 targetB = new Vector3(
            startB.x,
            squeegeeB.localPosition.y,
            forward
                ? startB.z + moveDistance
                : startB.z
        );
        while (Vector3.Distance(squeegeeA.localPosition, targetA) > 0.01f)
        {
            //Debug.Log("Moving");
            squeegeeA.localPosition = Vector3.MoveTowards(
                squeegeeA.localPosition,
                targetA,
                squeegeeSpeed * Time.deltaTime
            );
            squeegeeB.localPosition = Vector3.MoveTowards(
                squeegeeB.localPosition,
                targetB,
                squeegeeSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
}
