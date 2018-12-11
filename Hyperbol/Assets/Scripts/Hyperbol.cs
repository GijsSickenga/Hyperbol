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
    public float moveSpeedCap = 150f;
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

    [BoxGroup(MOVEMENT_TITLE)]
    public float wallOffset = 0.1f;

    // Visuals.
    [BoxGroup(VISUALS_TITLE)] [ColorUsage(false, true)]
    public Color defaultColor;
    [BoxGroup(VISUALS_TITLE)] [GradientHDR]
    public Gradient colorOverVelocity;
    [BoxGroup(VISUALS_TITLE)]
    public Light light;

    [BoxGroup(VISUALS_TITLE)]
    public GameObject wallBounce;
    [BoxGroup(VISUALS_TITLE)]
    public GameObject shootRedPS;
    [BoxGroup(VISUALS_TITLE)]
    public GameObject shootBluePS;

    [BoxGroup(VISUALS_TITLE)]
    public GameObject trailObject;
    private TrailRenderer trail;

    private float VelocityPortionOfMax
    {
        get
        {
            return (CurrentFlySpeed - baseMoveSpeed) / (moveSpeedCap - baseMoveSpeed);
        }
    }

    [HideInInspector]
    public Transform currentSpinTarget;

    public float colliderRadius = 0.5f;

    private float resetSpeedTimer;
    private float currentMaxSpeed;
    private float _currentFlySpeed;
    private float CurrentFlySpeed
    {
        get { return _currentFlySpeed; }
        set { _currentFlySpeed = Mathf.Clamp(value, 0, moveSpeedCap); }
    }
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
        if (trail != null)
        {
            trail.transform.parent = null;
            trail.autodestruct = true;
        }

        currentSpinTarget = null;
        transform.position = startPoint.position;

        CurrentFlySpeed = baseMoveSpeed;
        currentMaxSpeed = CurrentFlySpeed;
        currentDirection = Vector3.right;

        GameObject newTrailInstance = Instantiate(trailObject);
        trail = newTrailInstance.GetComponent<TrailRenderer>();
        trail.transform.parent = transform;
        trail.transform.localPosition = Vector3.zero;
    }

    public void GoRandomDirection()
    {
        Vector3 dir = Random.onUnitSphere;
        dir.y = 0;
        dir.Normalize();

        currentDirection = dir;
        currentMaxSpeed = baseMoveSpeed;
        CurrentFlySpeed = baseMoveSpeed;
    }

    public void SpeedUpBall(int percentage)
    {
        float factor = (100 + percentage) / 100f;

        currentMaxSpeed = CurrentFlySpeed * factor;
        CurrentFlySpeed = currentMaxSpeed;

        resetSpeedTimer = timeToResetSpeed;
    }

    public void ShootBall(Teams team)
    {
        Vector3 direction = transform.position - currentSpinTarget.position;

        direction.y = 0;
        direction.Normalize();

        currentSpinTarget = null;

        currentDirection = direction;
        SpeedUpBall(ShootSpeedIncreasePercentage);

        if (team == Teams.Red)
        {
            GameObject ps = Instantiate(shootRedPS, transform.position, Quaternion.LookRotation(direction, Vector3.up));
        }
        else if (team == Teams.Blue)
        {
            GameObject ps = Instantiate(shootBluePS, transform.position, Quaternion.LookRotation(direction, Vector3.up));
        }
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
        currentMaxSpeed = CurrentFlySpeed;
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

                CurrentFlySpeed = newSpeed;
            }
            else
            {
                CurrentFlySpeed = baseMoveSpeed;
                currentMaxSpeed = baseMoveSpeed;
            }

            // Handle default movement
            currentDirection.y = 0;
            currentDirection.Normalize();

            Vector3 moveVec = currentDirection * CurrentFlySpeed * Time.deltaTime;
            Vector3 ballEdgeOffset = currentDirection * colliderRadius + transform.position;

            Ray moveRay = new Ray(ballEdgeOffset, moveVec.normalized);
            RaycastHit hit;

            float distanceToTravel = CurrentFlySpeed * Time.deltaTime;
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

                    GameObject ps = Instantiate(wallBounce, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                    var bullshit = ps.GetComponent<ParticleSystem>().main;
                    bullshit.startColor = MeshRenderer.sharedMaterial.GetColor(EMISSIVE_COLOR_NAME);
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

            // Check outside map
            if (Mathf.Abs(transform.position.x) > 70f)
                ResetBal();
            if (Mathf.Abs(transform.position.z) > 70f)
                ResetBal();
        }
        else
        {
            if (resetSpeedTimer > 0)
            {
                resetSpeedTimer -= Time.deltaTime;

                float factor = dragCurve.Evaluate((timeToResetSpeedWhenHeld - resetSpeedTimer) / timeToResetSpeedWhenHeld);
                float newSpeed = baseMoveSpeed + (currentMaxSpeed - baseMoveSpeed) * factor;

                CurrentFlySpeed = newSpeed;
            }
            else
            {
                currentMaxSpeed = baseMoveSpeed;
            }

            float spinSpeed = CurrentFlySpeed / spinSpeedMultiplier;

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

            float xPos = Mathf.Cos(currentSpinAngle) * spinDistance;

            if(xPos < 0)
            {
                RaycastHit rayHit;
                if (Physics.Raycast(currentSpinTarget.position, Vector3.left, out rayHit, spinDistance))
                {
                    float distWithRadius = rayHit.distance - colliderRadius - wallOffset;
                    if (Mathf.Abs(xPos) > distWithRadius)
                        xPos = -distWithRadius;
                }
            }
            else
            {
                RaycastHit rayHit;
                if (Physics.Raycast(currentSpinTarget.position, Vector3.right, out rayHit, spinDistance))
                {
                    float distWithRadius = rayHit.distance - colliderRadius - wallOffset;
                    if (xPos > distWithRadius)
                        xPos = distWithRadius;
                }
            }

            float zPos = Mathf.Sin(currentSpinAngle) * spinDistance;

            if (zPos < 0)
            {
                RaycastHit rayHit;
                if (Physics.Raycast(currentSpinTarget.position, Vector3.back, out rayHit, spinDistance))
                {
                    float distWithRadius = rayHit.distance - colliderRadius - wallOffset;
                    if (Mathf.Abs(zPos) > distWithRadius)
                        zPos = -distWithRadius;
                }
            }
            else
            {
                RaycastHit rayHit;
                if (Physics.Raycast(currentSpinTarget.position, Vector3.forward, out rayHit, spinDistance))
                {
                    float distWithRadius = rayHit.distance - colliderRadius - wallOffset;
                    if (zPos > distWithRadius)
                        zPos = distWithRadius;
                }
            }

            Vector3 spinOffset = new Vector3(xPos, 0, zPos);

            transform.position = currentSpinTarget.position + spinOffset;
        }

        // Change color.
        Color gradientColor = colorOverVelocity.Evaluate(VelocityPortionOfMax);
        MeshRenderer.sharedMaterial.SetColor(EMISSIVE_COLOR_NAME, gradientColor);
        light.color = gradientColor;
	}
}
