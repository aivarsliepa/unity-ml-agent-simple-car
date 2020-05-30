using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject focus;
    public float distance = 8f;
    public float height = 4f;

    // Update is called once per frame
    void Update()
    {
        var offset = new Vector3(0f, height, -distance);
        //transform.position = Vector3.Lerp(transform.position, focus.transform.position + offset, Time.deltaTime);
        //transform.position = focus.transform.position + offset;
        transform.position = focus.transform.position + focus.transform.TransformDirection(offset);
        transform.LookAt(focus.transform);
    }
}
