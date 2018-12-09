using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperbol : MonoBehaviour
{
    public Transform startPoint;
    public float baseMoveSpeed = 10f;
    public AnimationCurve dragCurve;
    public float timeToResetSpeed = 5f;
    public float timeToResetSpeedWhenHeld = 5f;

    public float spinDistance = 1f;
    public float spinSpeedMultiplier = 5f;

    public int ShootSpeedIncreasePercentage = 50;

    [HideInInspector]
    public Transform currentSpinTarget;

    private Rigidbody rb;
    private float resetSpeedTimer;
    private float currentMaxSpeed;
    private float currentFlySpeed;
    private float currentSpinAngle;

    private float CircleDistance
    {
        get
        {
            return Mathf.PI * spinDistance * 2f;
        }
    }

	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        ResetBal();
	}

    public void ResetBal()
    {
        transform.position = startPoint.position;

        rb.velocity = Vector3.zero;
        currentFlySpeed = baseMoveSpeed;
        currentMaxSpeed = currentFlySpeed;
    }

    public void GoRandomDirection()
    {
        Vector3 dir = Random.onUnitSphere;
        dir.y = 0;
        dir.Normalize();

        rb.velocity = dir * baseMoveSpeed;
        currentMaxSpeed = baseMoveSpeed;
        currentFlySpeed = baseMoveSpeed;
    }

    public void SpeedUpBall(int percentage)
    {
        float factor = (100 + percentage) / 100f;

        currentMaxSpeed = rb.velocity.magnitude * factor;
        currentFlySpeed = currentMaxSpeed;

        resetSpeedTimer = timeToResetSpeed;
    }

    public void ShootBall()
    {
        Vector3 direction = transform.position - currentSpinTarget.position;

        direction.y = 0;
        direction.Normalize();

        currentSpinTarget = null;

        rb.velocity = direction * currentFlySpeed;
        SpeedUpBall(ShootSpeedIncreasePercentage);
    }

    public void ReceiveBall(Transform target)
    {
        rb.velocity = Vector3.zero;

        Vector3 direction = transform.position - target.position;
        direction.y = 0;
        direction.Normalize();

        currentSpinAngle = Mathf.Atan2(direction.z, direction.x);
        currentSpinTarget = target;
        resetSpeedTimer = timeToResetSpeedWhenHeld;
    }
	
	void Update ()
    {
        if (currentSpinTarget == null)
        {
            if (resetSpeedTimer > 0)
            {
                resetSpeedTimer -= Time.deltaTime;

                float factor = dragCurve.Evaluate((timeToResetSpeed - resetSpeedTimer) / timeToResetSpeed);
                float newSpeed = baseMoveSpeed + (currentMaxSpeed - baseMoveSpeed) * factor;

                rb.velocity = rb.velocity.normalized * newSpeed;
                currentFlySpeed = newSpeed;
            }
            else
            {
                currentFlySpeed = rb.velocity.magnitude;
            }
        }
        else
        {
            if (resetSpeedTimer > 0)
            {
                resetSpeedTimer -= Time.deltaTime;

                float factor = dragCurve.Evaluate((timeToResetSpeedWhenHeld - resetSpeedTimer) / timeToResetSpeedWhenHeld);
                float newSpeed = baseMoveSpeed + (currentMaxSpeed - baseMoveSpeed) * factor;

                currentFlySpeed = newSpeed;
            }

            float spinSpeed = currentFlySpeed / spinSpeedMultiplier;

            float timeForRotation = CircleDistance / spinSpeed;

            float rotationSpeed = Mathf.PI * 2 / timeForRotation;

            currentSpinAngle += rotationSpeed * Time.deltaTime;

            if(currentSpinAngle > Mathf.PI * 2)
            {
                currentSpinAngle -= Mathf.PI * 2;
            }
            else if (currentSpinAngle < 0)
            {
                currentSpinAngle += Mathf.PI * 2;
            }

            Vector3 spinOffset = new Vector3(Mathf.Cos(currentSpinAngle), 0, Mathf.Sin(currentSpinAngle));
            spinOffset *= spinDistance;

            transform.position = currentSpinTarget.position + spinOffset;
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(Tags.PLAYER))
        {
            ReceiveBall(other.transform);

            other.transform.root.GetComponent<LaunchBall>().PickUpBall(this);
        }
    }
}
