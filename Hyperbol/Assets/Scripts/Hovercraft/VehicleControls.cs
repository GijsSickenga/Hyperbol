using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine;
using XInputDotNetPure;

/// <summary>
/// Controls the vehicle with the specified inputs.
/// </summary>
[RequireComponent(typeof(VehicleStats))]
public class VehicleControls : MonoBehaviour
{
    private int _playerId = -1;
    private Teams _team;
    public Teams Team
    {
        get { return _team; }
    }

    public const float TRIGGER_TRESHOLD = 0.5f;
    public const float STICK_DEADZONE = 0.1333333f;

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

    private GamePadState _previousGamePadState;
    private GamePadState _currentGamePadState;

    // Control variables.
    private bool _leftHeld, _rightHeld,
                 _speedUpHeld, _reverseHeld,
                 _dischargePressed, _blinkPressed,
                 _launchBallPressed;

    // Debug keyboard buttons.
    public KeyCode leftButton, rightButton,
                   speedUpButton, reverseButton,
                   dischargeButton, blinkButton,
                   launchBallButton;

    public void Initialize(int id, Teams team)
    {
        _playerId = id;
        _team = team;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Debug.
        UpdateInputEditor();
#else
        // Check if gamepad is assigned.
        if (_playerId != -1)
        {
            UpdateInput();
        }
        else
        {
            return;
        }
#endif

        // Steering.
        if (_rightHeld && !_leftHeld)
        {
            VehicleStats.VehiclePhysics.SteerClockwise();
        }
        if (_leftHeld && !_rightHeld)
        {
            VehicleStats.VehiclePhysics.SteerCounterClockwise();
        }

        // Driving.
        if (_speedUpHeld && !_reverseHeld)
        {
            VehicleStats.VehiclePhysics.SpeedUp();
        }
        if (_reverseHeld)
        {
            VehicleStats.VehiclePhysics.SlowDown();
        }

        // Abilities.
        if (_dischargePressed)
        {
            // VehicleStats.Discharge.Invoke();
        }
        if (_blinkPressed)
        {
            // VehicleStats.Blink.Invoke();
        }
        if (_launchBallPressed)
        {
            VehicleStats.LaunchBall.ShootBall();
        }
    }

    private void UpdateInput()
    {
        // Update states.
        _previousGamePadState = _currentGamePadState;
        _currentGamePadState = GamePad.GetState((PlayerIndex)_playerId);

        // Steering.
        _leftHeld = _currentGamePadState.ThumbSticks.Left.X < -STICK_DEADZONE;
        _rightHeld = _currentGamePadState.ThumbSticks.Left.X > STICK_DEADZONE;

        // Driving.
        _speedUpHeld = _currentGamePadState.Triggers.Right >= TRIGGER_TRESHOLD;
        _reverseHeld = _currentGamePadState.Triggers.Left >= TRIGGER_TRESHOLD;

        // Abilities.
        _dischargePressed = _currentGamePadState.Buttons.X == ButtonState.Pressed && _previousGamePadState.Buttons.X == ButtonState.Released;
        _blinkPressed = _currentGamePadState.Buttons.B == ButtonState.Pressed && _previousGamePadState.Buttons.B == ButtonState.Released;
        _launchBallPressed = _currentGamePadState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug function that gets both controller and keyboard input.
    /// Keyboard input affects all players.
    /// </summary>
    private void UpdateInputEditor()
    {
        // Gamepad input.
        if (_playerId != -1)
        {
            UpdateInput();
        }
        else
        {
            // Debug keyboard input.
            // Steering.
            _leftHeld = Input.GetKey(leftButton);
            _rightHeld = Input.GetKey(rightButton);

            // Driving.
            _speedUpHeld = Input.GetKey(speedUpButton);
            _reverseHeld = Input.GetKey(reverseButton);

            // Abilities.
            _dischargePressed = Input.GetKeyDown(dischargeButton);
            _blinkPressed = Input.GetKeyDown(blinkButton);
            _launchBallPressed = Input.GetKeyDown(launchBallButton);
        }
    }
#endif
}
