using UnityEngine;
using System.Collections;

public class SqueegeeMovement : MonoBehaviour
{
    public float dropHeight = 0.1f; // Extra downward movement before moving
    public float liftHeight = 0.2f; // Moves up before stencil lifts
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    public IEnumerator DropSqueegee()
    {
        Vector3 lowerPosition = startPosition - new Vector3(0, dropHeight, 0);
        while (Vector3.Distance(transform.position, lowerPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, lowerPosition, Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator LiftSqueegee()
    {
        Vector3 upperPosition = transform.position + new Vector3(0, liftHeight, 0);
        while (Vector3.Distance(transform.position, upperPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, upperPosition, Time.deltaTime);
            yield return null;
        }
    }
}
