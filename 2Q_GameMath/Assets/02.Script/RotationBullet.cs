using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBullet : MonoBehaviour
{
    private Vector3 rotationDir = new Vector3(0,0,1);
    private float rotationSpeed = 1200;

    private void Update()
    {
        float deltaAngle = rotationSpeed * Time.deltaTime;
        this.transform.localRotation *= Quaternion.Euler(rotationDir * deltaAngle);
    }
}
