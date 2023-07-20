using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    

    private void LateUpdate()
    {
        transform.position = new(playerTransform.position.x, playerTransform.position.y, -10);
    }
   
}
