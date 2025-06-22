using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASM_SMT_Lid_Open : MonoBehaviour
{
    public Animator lidAnimator;
    private bool isOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.enabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("ToggleButton2"))
                {
                    isOpen = !isOpen;
                    lidAnimator.SetBool("IsOpen", isOpen);
                }
            }
        }
    }
}
