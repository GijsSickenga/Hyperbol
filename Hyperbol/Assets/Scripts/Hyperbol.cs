using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Hyperbol : MonoBehaviour
{
    private const string MOVEMENT_TITLE = "Movement";
    private const string VISUALS_TITLE = "Visuals";

    private const string EMISSIVE_COLOR_NAME = "_EmissionColor";

    private MeshRenderer _meshRenderer;
    private MeshRenderer MeshRenderer
    {
        get
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
            return _meshRenderer;
        }
    }

    // Math.
    [BoxGroup(MOVEMENT_TITLE)]
    public Transform startPoint;
    [BoxGroup(MOVEMENT_TITLE)]
    public float baseMoveSpeed = 10f;
    [BoxGroup(MOVEMENT_TITLE)]
    public AnimationCurve dragCurve;
    [BoxGroup(MOVEMENT_TITLE)]
    public float timeToResetSpeed = 5f;
    [BoxGroup(MOVEMENT_TITLE)]
    public float timeToResetSpeedWhenHeld = 5f;

    [BoxGroup(MOVEMENT_TITLE)]
    public float spinDistance = 1f;
    [BoxGroup(MOVEMENT_TITLE)]
    public float spinSpeedMultiplier = 5f;

    [BoxGroup(MOVEMENT_TITLE)]
    public int ShootSpeedIncreasePercentage = 50;

    // Visuals.
    [BoxGroup(VISUALS_TITLE)] [ColorUsage(false, true)]
    public Color defaultColor;
    [BoxGroup(VISUALS_TITLE)] [GradientHDR]
    public Gradient colorOverVelocity;
    [BoxGroup(VISUALS_TITLE)]
    public float maxColorVelocity = 100;
    [BoxGroup(VISUALS_TITLE)]
    public Light light;

    private float VelocityPortionOfMax
    {
        get
        {
            return currentFlySpeed / maxColorVelocity;
        }
    }

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

	void Start()
    {
        rb = GetComponent<Rigidbody>();

        ResetBal();
	}

    private void OnDisable()
    {
        MeshRenderer.sharedMaterial.SetColor(EMISSIVE_COLOR_NAME, defaultColor);
        light.color = defaultColor;
    }

    public void ResetBal()
    {
        currentSpinTarget = null;
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
	
	void Update()
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

            if (currentSpinAngle > Mathf.PI * 2)
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

        // Change color.
        Color gradientColor = colorOverVelocity.Evaluate(VelocityPortionOfMax);
        MeshRenderer.sharedMaterial.SetColor(EMISSIVE_COLOR_NAME, gradientColor);
        light.color = gradientColor;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.PLAYER))
        {
            ReceiveBall(other.transform);

            other.transform.root.GetComponent<LaunchBall>().PickUpBall(this);
        }
    }
}
