using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deathscript : MonoBehaviour
{
    public Transform spawnPoint;
    [SerializeField] private Transform deathCheck;
    [SerializeField] private LayerMask deathlayer;
    [SerializeField] private Rigidbody2D rb;

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("death"))
        {
            transform.position = new(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z); 
        }
    }*/

    private void Update()
    {
        if (rb.IsTouchingLayers(deathlayer.value))
        {
            Debug.Log("Touched");
            transform.position = new(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);
        }
    }





    private bool IsDead() 
    {
        return Physics2D.OverlapCircle(deathCheck.position, 1f, deathlayer.value);
    }


}
