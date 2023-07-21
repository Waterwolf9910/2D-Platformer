using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class patroler : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 point =currentPoint.position;
        if(currentPoint == pointB.transform) 
        {
            rb.velocity = new Vector2(speed, 0);

        }
        else
        {
            rb .velocity = new Vector2(-speed, 0);
        }
    if(Vector2.Distance(transform.position, currentPoint.position) <1f && currentPoint == pointB.transform)
        {
            currentPoint = pointA.transform;
        }
     if(Vector2.Distance(transform.position, currentPoint.position) < 1f && currentPoint == pointA.transform)
        {
            currentPoint = pointB.transform;
        }
    }

    private void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.DrawWireSphere(pointA.transform.position, .5f);
            Gizmos.DrawWireSphere(pointB.transform.position, .5f);
        }
    }
}
