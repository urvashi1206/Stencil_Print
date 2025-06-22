using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiActor))]
public class ColorFromVelocity3D_ToggleAxes : MonoBehaviour
{
    ObiActor actor;

    [Header("Axis Color Mapping")]
    public Color rightColor = Color.red;
    public Color upColor = Color.green;
    public Color forwardColor = Color.blue;

    [Header("Sensitivity / Scale")]
    public float sensibility = 1f;

    [Header("Toggle Axes")]
    public bool useX = true;
    public bool useY = true;
    public bool useZ = true;
    public bool active = false;
    void Awake()
    {
        actor = GetComponent<ObiActor>();
    }

    void LateUpdate()
    {
        if (!isActiveAndEnabled || actor.solver == null || !active) return;

        int count = actor.solverIndices.count;
        for (int i = 0; i < count; ++i)
        {
            int k = actor.solverIndices[i];
            Vector3 v = (Vector3)actor.solver.velocities[k] / sensibility;

            // clamp so we only pick up to 1.0
            float vx = useX ? Mathf.Clamp01(v.x) : 0f;
            float vy = useY ? Mathf.Clamp01(v.y) : 0f;
            float vz = useZ ? Mathf.Clamp01(v.z) : 0f;

            // blend the chosen axis-colors
            Color col = rightColor * vx
                      + upColor * vy
                      + forwardColor * vz;

            actor.solver.colors[k] = col;
        }
    }
}
