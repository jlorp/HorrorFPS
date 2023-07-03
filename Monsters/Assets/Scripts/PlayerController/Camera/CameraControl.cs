using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField, Range(-89f, 89f)]float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    Vector2 orbitAngles = new Vector2(45f, 0f);
    [SerializeField, Range(1f, 360f)]float rotationSpeed = 90f;
    float lastManualRotationTime;

    void Awake()
    {
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    void LateUpdate()
    {
        Quaternion lookRotation;
        if (ManualRotation()) {
			ConstrainAngles();
			lookRotation = Quaternion.Euler(orbitAngles);
		}
		else {
			lookRotation = transform.localRotation;
		}
        transform.rotation= lookRotation;

    }

    bool ManualRotation () {
		Vector2 input = new Vector2(
			Input.GetAxis("VerticalAim"),
			Input.GetAxis("HorizontalAim")
		);
		const float e = 0.001f;
		if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
			orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
		}
        return false;
	}  

     void ConstrainAngles () {
		orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (orbitAngles.y < 0f) {
			orbitAngles.y += 360f;
		}
		else if (orbitAngles.y >= 360f) {
			orbitAngles.y -= 360f;
		}
	}
}
