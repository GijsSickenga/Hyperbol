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

    public float colliderRadius = 0.5f;

    private float resetSpeedTimer;
    private float currentMaxSpeed;
    private float currentFlySpeed;
    private float currentSpinAngle;

    private Vector3 currentDirection;

    private float CircleDistance
    {
        get
        {
            return Mathf.PI * spinDistance * 2f;
        }
    }

	void Start()
    {
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

        currentFlySpeed = baseMoveSpeed;
        currentMaxSpeed = currentFlySpeed;
        currentDirection = Vector3.right;
    }

    public void GoRandomDirection()
    {
        Vector3 dir = Random.onUnitSphere;
        dir.y = 0;
        dir.Normalize();

        currentDirection = dir;
        currentMaxSpeed = baseMoveSpeed;
        currentFlySpeed = baseMoveSpeed;
    }

    public void SpeedUpBall(int percentage)
    {
        float factor = (100 + percentage) / 100f;

        currentMaxSpeed = currentFlySpeed * factor;
        currentFlySpeed = currentMaxSpeed;

        resetSpeedTimer = timeToResetSpeed;
    }

    public void ShootBall()
    {
        Vector3 direction = transform.position - currentSpinTarget.position;

        direction.y = 0;
        direction.Normalize();

        currentSpinTarget = null;

        currentDirection = direction;
        SpeedUpBall(ShootSpeedIncreasePercentage);
    }

    public void ReceiveBall(Transform target)
    {
        currentDirection = Vector3.zero;

        Vector3 direction = transform.position - target.position;
        direction.y = 0;
        direction.Normalize();

        currentSpinAngle = Mathf.Atan2(direction.z, direction.x);
        currentSpinTarget = target;

        resetSpeedTimer = timeToResetSpeedWhenHeld;
        currentMaxSpeed = currentFlySpeed;
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

                currentFlySpeed = newSpeed;
            }
            else
            {
                currentFlySpeed = baseMoveSpeed;
                currentMaxSpeed = baseMoveSpeed;
            }

            // Handle default movement
            currentDirection.y = 0;
            currentDirection.Normalize();

            Vector3 moveVec = currentDirection * currentFlySpeed * Time.deltaTime;
            Vector3 ballEdgeOffset = currentDirection * colliderRadius + transform.position;

            Ray moveRay = new Ray(ballEdgeOffset, moveVec.normalized);
            RaycastHit hit;

            float distanceToTravel = currentFlySpeed * Time.deltaTime;
            Vector3 newPosition = ballEdgeOffset;

            bool overrideMovement = false;

            while (Physics.Raycast(moveRay, out hit, distanceToTravel) && !overrideMovement)
            {
                if (hit.transform.CompareTag(Tags.WALL))
                {
                    // Wall, reflect
                    Vector3 reflected = Vector3.Reflect(currentDirection, hit.normal);
                    reflected.y = 0;
                    reflected.Normalize();
                    moveRay = new Ray(hit.point, reflected);

                    distanceToTravel -= hit.distance;
                    reflected *= distanceToTravel;

                    Debug.DrawLine(newPosition, hit.point,Color.red, 5f);
                    currentDirection = reflected.normalized;
                    newPosition = hit.point;
                } 
                else if (hit.transform.CompareTag(Tags.GOAL))
                {
                    hit.transform.GetComponent<Goal>().Score();

                    ResetBal();
                    overrideMovement = true;
                }
                else if (hit.transform.CompareTag(Tags.PLAYER))
                {
                    ReceiveBall(hit.transform);

                    hit.transform.root.GetComponent<LaunchBall>().PickUpBall(this);

                    overrideMovement = true;
                }
            }

            if (!overrideMovement)
            {
                // Nothing hit, can move freely
                transform.position = newPosition + currentDirection * distanceToTravel - currentDirection * colliderRadius;
                Debug.DrawLine(newPosition, newPosition + currentDirection * distanceToTravel, Color.red, 5f);
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
            else
            {
                currentMaxSpeed = baseMoveSpeed;
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
        
    }
}
