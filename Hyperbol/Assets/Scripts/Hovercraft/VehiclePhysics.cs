using System.Collections;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Handles how the vehicle is affected by physics.
/// </summary>
public class VehiclePhysics : MonoBehaviour
{
    private const string READ_ONLY_BOX_TITLE = "Read Only Values";
    private const string DRIVING_BOX_TITLE = "Driving";
    private const string STEERING_BOX_TITLE = "Steering";
    private const string SWAY_BOX_TITLE = "Sway";

    private VehicleStats _vehicleStats;
    public VehicleStats VehicleStats
    {
        get
        {
            if (_vehicleStats == null)
            {
                _vehicleStats = GetComponent<VehicleStats>();
            }
            return _vehicleStats;
        }
    }

    #region Boolean Properties
    /// <summary>
    /// Whether the vehicle responds to steering input.
    /// </summary>
    public bool CanReceiveInput
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the vehicle is currently turning clockwise or counter-clockwise.
    /// </summary>
    public bool IsTurning
    {
        get;
        private set;
    }

    private bool _isBeingSpedUp = false;
    private bool _isBeingSpedUpThisFrame = false;
    /// <summary>
    /// Whether the vehicle is currently being sped up from outside VehiclePhysics.
    /// </summary>
    public bool IsBeingSpedUp
    {
        get { return _isBeingSpedUp; }
        private set
        {
            _isBeingSpedUp = value;
            if (IsBeingSpedUp)
            {
                _isBeingSpedUpThisFrame = true;
            }
        }
    }

    /// <summary>
    /// Whether the vehicle is currently being steered from outside VehiclePhysics.
    /// </summary>
    public bool IsBeingSteered
    {
        get { return IsBeingSteeredClockwise || IsBeingSteeredCounterClockwise; }
    }

    private bool _isBeingSteeredClockwise = false;
    private bool _isBeingSteeredClockwiseThisFrame = false;
    /// <summary>
    /// Whether the vehicle is currently being steered clockwise from outside VehiclePhysics.
    /// </summary>
    public bool IsBeingSteeredClockwise
    {
        get { return _isBeingSteeredClockwise; }
        private set
        {
            _isBeingSteeredClockwise = value;
            if (IsBeingSteeredClockwise)
            {
                _isBeingSteeredClockwiseThisFrame = true;
            }
        }
    }

    private bool _isBeingSteeredCounterClockwise = false;
    private bool _isBeingSteeredCounterClockwiseThisFrame = false;
    /// <summary>
    /// Whether the vehicle is currently being steered counter-clockwise from outside VehiclePhysics.
    /// </summary>
    public bool IsBeingSteeredCounterClockwise
    {
        get { return _isBeingSteeredCounterClockwise; }
        private set
        {
            _isBeingSteeredCounterClockwise = value;
            if (IsBeingSteeredCounterClockwise)
            {
                _isBeingSteeredCounterClockwiseThisFrame = true;
            }
        }
    }
    #endregion

    #region Current & Read Only Values
    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current driving velocity of the vehicle.")]
    private float _drivingVelocity = 0f;
    /// <summary>
    /// The current driving velocity of the vehicle.
    /// </summary>
    public float DrivingVelocity
    {
        get { return _drivingVelocity; }
        set { _drivingVelocity = Mathf.Clamp(value, -MaxDrivingVelocity, MaxDrivingVelocity); }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current braking velocity of the vehicle.")]
    private float _brakingVelocity = 0f;
    /// <summary>
    /// The current braking velocity of the vehicle.
    /// </summary>
    public float BrakingVelocity
    {
        get { return _brakingVelocity; }
        set { _brakingVelocity = Mathf.Clamp(value, -MaxDrivingVelocity, MaxDrivingVelocity); }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The time in seconds it takes the vehicle to stop moving from max velocity through friction.")]
    private float _drivingSlowdownTime = 0f;
    /// <summary>
    /// The time in seconds it takes the vehicle to stop moving from max velocity through friction.
    /// </summary>
    public float SailingSlowdownTime
    {
        get { return _drivingSlowdownTime; }
    }

    // Place some space between different variable categories in the inspector.
    [Space(5)]

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The current turning velocity of the vehicle.")]
    private float _rotationalVelocity = 0f;
    /// <summary>
    /// The current turning velocity of the vehicle.
    /// Negative when turning counter-clockwise.
    /// </summary>
    public float RotationalVelocity
    {
        get { return _rotationalVelocity; }
        private set
        {
            _rotationalVelocity = Mathf.Clamp(value, -MaxRotationalVelocity, MaxRotationalVelocity);
            IsTurning = _rotationalVelocity != 0 ? true : false;
        }
    }

    [BoxGroup(READ_ONLY_BOX_TITLE)]
    [SerializeField] [ReadOnly]
    [Tooltip("The time in seconds it takes the vehicle to stop turning from max turning velocity through friction.")]
    private float _turningSlowdownTime = 0f;
    /// <summary>
    /// The time in seconds it takes the vehicle to stop turning from max turning velocity through friction.
    /// </summary>
    public float TurningSlowdownTime
    {
        get { return _turningSlowdownTime; }
    }
    #endregion

    #region Driving Settings
    [BoxGroup(DRIVING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum driving velocity of the vehicle in unity units per second.")]
    private float _maxDrivingVelocity = 40f;
    /// <summary>
    /// The maximum driving velocity of the vehicle.
    /// </summary>
    public float MaxDrivingVelocity
    {
        get { return _maxDrivingVelocity; }
        private set
        {
            _maxDrivingVelocity = ExFuncs.ClampPositive(value);
            UpdateDrivingSlowdownTime();
        }
    }

    [BoxGroup(DRIVING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum reverse velocity of the vehicle in unity units per second.")]
    private float _maxReverseVelocity = 40f;
    /// <summary>
    /// The maximum reverse velocity of the vehicle.
    /// </summary>
    public float MaxReverseVelocity
    {
        get { return _maxReverseVelocity; }
        private set { _maxReverseVelocity = ExFuncs.ClampPositive(value); }
    }

    /// <summary>
    /// Returns current driving velocity portion of max driving velocity.
    /// 1 = maxed out velocity.
    /// </summary>
    public float DrivingVelocityPortionOfMax
    {
        get { return VehicleStats.VehicleRigidbody.velocity.magnitude / MaxDrivingVelocity; }
    }

    [BoxGroup(DRIVING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The driving acceleration of the vehicle in unity units per second.")]
    private float _drivingAcceleration = 15f;
    /// <summary>
    /// The driving acceleration of the vehicle.
    /// </summary>
    public float DrivingAcceleration
    {
        get { return _drivingAcceleration; }
        private set { _drivingAcceleration = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(DRIVING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The braking acceleration of the vehicle in unity units per second.")]
    private float _brakingAcceleration = 15f;
    /// <summary>
    /// The braking acceleration of the vehicle.
    /// </summary>
    public float BrakingAcceleration
    {
        get { return _brakingAcceleration; }
        private set { _brakingAcceleration = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(DRIVING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("How much friction is applied to the driving velocity per second.")]
    private float _drivingFriction = 1.3333333f;
    /// <summary>
    /// How much friction is applied to the driving velocity per second.
    /// </summary>
    public float DrivingFriction
    {
        get { return _drivingFriction; }
        private set
        {
            _drivingFriction = ExFuncs.ClampPositive(value);
            UpdateDrivingSlowdownTime();
        }
    }

    /// <summary>
    /// Updates the driving slowdown time based on max velocity and friction.
    /// </summary>
    private void UpdateDrivingSlowdownTime()
    {
        _drivingSlowdownTime = ExFuncs.RoundToDecimal(MaxDrivingVelocity / DrivingFriction, 2);
    }
    #endregion

    #region Steering Settings
    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum rotational velocity of the vehicle in degrees per second.")]
    private float _maxRotationalVelocity = 50f;
    /// <summary>
    /// The maximum rotational velocity of the vehicle.
    /// </summary>
    public float MaxRotationalVelocity
    {
        get { return _maxRotationalVelocity; }
        private set
        {
            _maxRotationalVelocity = ExFuncs.ClampPositive(value);
            UpdateTurningSlowdownTime();
        }
    }

    /// <summary>
    /// Returns current rotational velocity portion of max rotational velocity.
    /// 1 = maxed out velocity, clockwise.
    /// -1 = maxed out velocity, counter-clockwise.
    /// </summary>
    public float RotationalVelocityPortionOfMax
    {
        get { return RotationalVelocity / MaxRotationalVelocity; }
    }

    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The rotational acceleration of the vehicle in degrees per second.")]
    private float _rotationalAcceleration = 140f;
    /// <summary>
    /// The rotational acceleration of the vehicle in degrees per second.
    /// </summary>
    public float RotationalAcceleration
    {
        get { return _rotationalAcceleration; }
        private set { _rotationalAcceleration = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(STEERING_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("How much friction is applied to the rotational velocity per second.")]
    private float _turningFriction = 25f;
    /// <summary>
    /// How much friction is applied to the rotational velocity per second.
    /// </summary>
    public float TurningFriction
    {
        get { return _turningFriction; }
        private set
        {
            _turningFriction = ExFuncs.ClampPositive(value);
            UpdateTurningSlowdownTime();
        }
    }

    /// <summary>
    /// Updates the turning slowdown time based on max velocity and friction.
    /// </summary>
    private void UpdateTurningSlowdownTime()
    {
        _turningSlowdownTime = ExFuncs.RoundToDecimal(MaxRotationalVelocity / TurningFriction, 2);
    }
    #endregion

    #region Steering Sway Settings
    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinValue(0)]
    [Tooltip("The maximum rotation the vehicle can keel over on its roll (Z) axis by turning at max sail & rotational velocities.")]
    private float _maxRoll = 10f;
    /// <summary>
    /// The maximum rotation the vehicle can keel over on its roll (Z) axis by turning at max sail & rotational velocities.
    /// </summary>
    public float MaxRoll
    {
        get { return _maxRoll; }
        private set { _maxRoll = ExFuncs.ClampPositive(value); }
    }

    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinMaxSlider(0, 1)]
    [Tooltip("Determines the range over which the driving velocity affects the sway angle.")]
    private Vector2 _drivingSwayRange = new Vector2(0, 1);
    /// <summary>
    /// Determines the range over which the driving velocity affects the sway angle.
    /// </summary>
    public Vector2 DrivingSwayRange
    {
        get { return _drivingSwayRange; }
        private set
        {
            float lowerValue = Mathf.Min(value.x, value.y);
            float higherValue = Mathf.Max(value.x, value.y);
            _drivingSwayRange = new Vector2(Mathf.Clamp01(lowerValue), Mathf.Clamp01(higherValue));
        }
    }

    [BoxGroup(SWAY_BOX_TITLE)]
    [SerializeField] [MinMaxSlider(0, 1)]
    [Tooltip("Determines the range over which the rotational velocity affects the sway angle.")]
    private Vector2 _turnSwayRange = new Vector2(0, 1);
    /// <summary>
    /// Determines the range over which the rotational velocity affects the sway angle.
    /// </summary>
    public Vector2 TurnSwayRange
    {
        get { return _turnSwayRange; }
        private set
        {
            float lowerValue = Mathf.Min(value.x, value.y);
            float higherValue = Mathf.Max(value.x, value.y);
            _turnSwayRange = new Vector2(Mathf.Clamp01(lowerValue), Mathf.Clamp01(higherValue));
        }
    }

    /// <summary>
    /// The roll (Z) rotation for the vehicle model when it is in neutral position.
    /// Steering sway uses this value as its centerpoint.
    /// </summary>
    public float NeutralRoll
    {
        get;
        private set;
    }

    /// <summary>
    /// Maps sail velocity portion of max to user set range.
    /// </summary>
    private float DrivingSwayScalar
    {
        get { return ExFuncs.DistanceAcrossRange(DrivingSwayRange, DrivingVelocityPortionOfMax); }
    }

    /// <summary>
    /// Maps rotational velocity portion of max to user set range.
    /// </summary>
    private float TurnSwayScalar
    {
        get { return Mathf.Sign(-RotationalVelocity) * ExFuncs.DistanceAcrossRange(TurnSwayRange, Mathf.Abs(RotationalVelocityPortionOfMax)); }
    }

    /// <summary>
    /// The sway cap in degrees for the vehicle while turning.
    /// The faster the vehicle goes, the more it can sway, up to MaxRoll degrees, in both directions.
    /// </summary>
    public float SwayCap
    {
        get { return MaxRoll * DrivingSwayScalar; }
    }

    /// <summary>
    /// Returns the sway angle as derived from max angle, current driving velocity and current rotational velocity.
    /// Takes neutral angle into account.
    /// </summary>
    private float DesiredSwayAngle
    {
        get { return NeutralRoll + SwayCap * TurnSwayScalar; }
    }
    #endregion

    #region Steering & Driving Functions
    /// <summary>
    /// Sets the rotational velocity of the vehicle to turn clockwise.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1, determining how much of max rotational acceleration should be used to steer.</param>   
    public void SteerClockwise(float portionOfMax = 1)
    {
        if (!CanReceiveInput)
            return;

        // Adjust rotational velocity.
        portionOfMax = Mathf.Clamp01(portionOfMax);
        RotationalVelocity += RotationalAcceleration * Time.deltaTime * portionOfMax;
        IsBeingSteeredClockwise = true;
    }

    /// <summary>
    /// Sets the rotational velocity of the vehicle to turn counter-clockwise.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1, determining how much of max rotational acceleration should be used to steer.</param>   
    public void SteerCounterClockwise(float portionOfMax = 1)
    {
        if (!CanReceiveInput)
            return;

        // Adjust rotational velocity.
        portionOfMax = Mathf.Clamp01(portionOfMax);
        RotationalVelocity -= RotationalAcceleration * Time.deltaTime * portionOfMax;
        IsBeingSteeredCounterClockwise = true;
    }

    /// <summary>
    /// Applies the driving acceleration to the driving velocity.
    /// </summary>
    public void SpeedUp()
    {
        if (!CanReceiveInput)
            return;

        DrivingVelocity += DrivingAcceleration * Time.deltaTime;
        IsBeingSpedUp = true;
    }

    /// <summary>
    /// Applies friction to the driving velocity to stop the vehicle.
    /// </summary>
    public void SlowDown()
    {
        if (!CanReceiveInput)
            return;

        BrakingVelocity += BrakingAcceleration * Time.deltaTime;
    }
    #endregion

    #region Specific Physics Events
    //________________________________________ QuickStop ________________________________________//
    private Coroutine _quickStopRoutine = null;
    /// <summary>
    /// Makes the vehicle come to a stop over time.
    /// </summary>
    /// <param name="stopTime">Time in seconds it should take to slow down from max speed.</param>
    public void QuickStop(float stopTime)
    {
        if (_quickStopRoutine == null)
        {
            _quickStopRoutine = StartCoroutine(QuickStopRoutine(stopTime));
        }
    }

    /// <summary>
    /// Adjusts the vehicle's friction so it gradually comes to a halt.
    /// </summary>
    /// <param name="maxStopTime">The time it takes the vehicle to come to a halt from max velocity.</param>
    private IEnumerator QuickStopRoutine(float maxStopTime)
    {
        // Cache default friction to reset to at the end.
        float previousFriction = DrivingFriction;
        float sailVelocityPortion = DrivingVelocity / MaxDrivingVelocity;

        // Set friction and wait until fully stopped.
        DrivingFriction = MaxDrivingVelocity / maxStopTime;
        yield return new WaitForSeconds(maxStopTime * sailVelocityPortion);

        // Nullify remaining velocity to make sure we've fully stopped.
        VehicleStats.VehicleRigidbody.velocity = Vector3.zero;

        // Reset to default friction.
        DrivingFriction = previousFriction;
        _quickStopRoutine = null;
        yield break;
    }

    //________________________________________ SetDrivingVelocityPortionOfMax ________________________________________//
    /// <summary>
    /// Sets the driving velocity as a given portion of max driving velocity.
    /// </summary>
    /// <param name="portionOfMax">Value between 0 and 1.</param>
    public void SetDrivingVelocityPortionOfMax(float portionOfMax)
    {
        // Driving velocity is automatically clamped between 0 and max.
        DrivingVelocity = MaxDrivingVelocity * portionOfMax;
    }

    //________________________________________ StopMoving ________________________________________//
    /// <summary>
    /// Stops the vehicle from moving instantly.
    /// </summary>
    public void StopMoving()
    {
        // Set velocities to zero.
        DrivingVelocity = 0;
        RotationalVelocity = 0;

        // Nullify remaining velocity.
        VehicleStats.VehicleRigidbody.velocity = Vector3.zero;
    }
    #endregion

    #region Physics Calculations
    /// <summary>
    /// Applies friction to the given variable.
    /// Friction is value per second.
    /// </summary>
    /// <param name="value">The value to apply friction to.</param>
    /// <param name="frictionAmountPerSecond">The amount of friction per second.</param>
    /// <param name="deltaTime">The timestep to use in calculating the magnitude of the friction.</param>
    private float ApplyFriction(float value, float frictionAmountPerSecond, float deltaTime)
    {
        // Calculate friction amount.
        float frictionAmount = frictionAmountPerSecond * deltaTime;
        // Take the smallest of calculated friction amount and remainder of value.
        frictionAmount = Mathf.Min(frictionAmount, Mathf.Abs(value));
        // Take sign of value into account so friction is applied towards 0.
        float targetValue = value - frictionAmount * Mathf.Sign(value);

        return targetValue;
    }

    /// <summary>
    /// Applies friction to the given variable.
    /// Friction is total time to slow down from max.
    /// </summary>
    /// <param name="value">The value to apply friction to.</param>
    /// <param name="maxValue">The maximum value of the variable that friction is being applied to.</param>
    /// <param name="degradationTimeFromMax">The time it takes for the value to reach 0 from max in seconds through friction.</param>
    /// <param name="deltaTime">The timestep to use in calculating the magnitude of the friction.</param>
    private float ApplyFriction(float value, float maxValue, float degradationTimeFromMax, float deltaTime)
    {
        // Calculate current and degradation values as portion of max value.
        float currentPortionOfMax = Mathf.Abs(value) / maxValue;
        float portionOfMaxToDegrade = 1 / degradationTimeFromMax * deltaTime;

        // Subtract degradation amount from current amount and clamp.
        float targetPortionOfMax = currentPortionOfMax - portionOfMaxToDegrade;
        targetPortionOfMax = Mathf.Clamp01(targetPortionOfMax);

        // Set target value as portion of max value.
        float targetValue = maxValue * targetPortionOfMax * Mathf.Sign(value);

        return targetValue;
    }

    /// <summary>
    /// Applies the current driving velocity to the rigidbody.
    /// </summary>
    private void ApplyDrivingVelocity()
    {
        VehicleStats.VehicleRigidbody.velocity += transform.forward * DrivingVelocity;
        DrivingVelocity = 0;
        VehicleStats.VehicleRigidbody.velocity = Vector3.ClampMagnitude(VehicleStats.VehicleRigidbody.velocity, MaxDrivingVelocity);
    }

    /// <summary>
    /// Applies the current braking velocity to the rigidbody.
    /// </summary>
    private void ApplyBrakingVelocity()
    {
		if (VehicleStats.VehicleRigidbody.velocity.magnitude > MaxReverseVelocity)
        {
            VehicleStats.VehicleRigidbody.velocity -= transform.forward * BrakingVelocity;
            // VehicleStats.VehicleRigidbody.velocity -= VehicleStats.VehicleRigidbody.velocity.normalized * BrakingVelocity;
            VehicleStats.VehicleRigidbody.velocity = Vector3.ClampMagnitude(VehicleStats.VehicleRigidbody.velocity, MaxDrivingVelocity);
        }
		else
        {
            VehicleStats.VehicleRigidbody.velocity -= transform.forward * BrakingVelocity;
            VehicleStats.VehicleRigidbody.velocity = Vector3.ClampMagnitude(VehicleStats.VehicleRigidbody.velocity, MaxReverseVelocity);
		}
        BrakingVelocity = 0;
    }

    /// <summary>
    /// Applies the current rotational velocity to the transform.
    /// Only call from FixedUpdate().
    /// </summary>
    private void ApplyRotationalVelocity()
    {
        float deltaAngle = RotationalVelocity * Time.fixedDeltaTime;
        // Yaw axis (Y) determines steering direction.
        transform.Rotate(0, deltaAngle, 0, Space.World);
    }

    /// <summary>
    /// Applies sway to the vehicle depending on how fast it is moving and turning.
    /// Only call from FixedUpdate().
    /// </summary>
    private void ApplySway()
    {
        float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.z, DesiredSwayAngle);
        // Roll axis (Z) determines sway.
        transform.Rotate(0, 0, deltaAngle, Space.Self);
    }
    #endregion

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Start()
    {
        CanReceiveInput = true;
        NeutralRoll = transform.eulerAngles.z;
    }

    /// <summary>
    /// Reset some variables.
    /// </summary>
    private void LateUpdate()
    {
        if (!_isBeingSpedUpThisFrame)
        {
            IsBeingSpedUp = false;
        }
        if (!_isBeingSteeredClockwiseThisFrame)
        {
            IsBeingSteeredClockwise = false;
        }
        if (!_isBeingSteeredCounterClockwiseThisFrame)
        {
            IsBeingSteeredCounterClockwise = false;
        }

        _isBeingSpedUpThisFrame = false;
        _isBeingSteeredClockwiseThisFrame = false;
        _isBeingSteeredCounterClockwiseThisFrame = false;
    }

    /// <summary>
    /// Handle physics calculations.
    /// </summary>
    private void FixedUpdate()
    {
        // Apply friction.
        // if (!IsBeingSpedUp)
        // {
        //     DrivingVelocity = ApplyFriction(DrivingVelocity, DrivingFriction, Time.fixedDeltaTime);
        // }
        if (!IsBeingSteered)
        {
            RotationalVelocity = ApplyFriction(RotationalVelocity, TurningFriction, Time.fixedDeltaTime);
        }

        // Apply calculated velocities to vehicle rigidbody.
        ApplyDrivingVelocity();
        ApplyBrakingVelocity();
        ApplyRotationalVelocity();
        ApplySway();
    }

    /// <summary>
    /// Perform logic after inspector update.
    /// </summary>
    private void OnValidate()
    {
        // Update values.
        UpdateDrivingSlowdownTime();
        UpdateTurningSlowdownTime();
    }
}
