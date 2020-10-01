using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planes : MonoBehaviour
{
    float xAngel = 0, yAngel = 0;
    void Update()
    {
        transform.position += (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized * 20 * Time.deltaTime;
        yAngel = Input.GetAxis("Mouse X");
        transform.Rotate(transform.up, yAngel);
        xAngel = Input.GetAxis("Mouse Y");
    }
}
