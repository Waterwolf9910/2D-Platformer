using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    [SerializeField] private GameObject One;
    [SerializeField] private GameObject Two;
    private float timeout = 0;
    // Start is called before the first frame update
    void Start() {
        One.GetComponent<EnterPortal>().Trigger += (collider) => {
            if (Time.realtimeSinceStartup - timeout > 1) {
                collider.transform.position = Two.transform.position;
                timeout = Time.realtimeSinceStartup;
            }
        };
        Two.GetComponent<EnterPortal>().Trigger += (collider) => {
            if (Time.realtimeSinceStartup - timeout > 1) {
                collider.transform.position = One.transform.position;
                timeout = Time.realtimeSinceStartup;
            }
        };
    }
}
