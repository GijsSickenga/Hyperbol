using System.Collections;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Manages the vehicle's stats and contains references to other vehicle scripts.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(VehiclePhysics))]
public class VehicleStats : MonoBehaviour
{
    private int _playerId;
    // private Teams _team;

    #region Miscellaneous Scripts
    private Rigidbody _vehicleRigidbody;
    public Rigidbody VehicleRigidbody
    {
        get
        {
            if (_vehicleRigidbody == null)
            {
                _vehicleRigidbody = GetComponent<Rigidbody>();
            }
            return _vehicleRigidbody;
        }
    }
    #endregion

    #region Vehicle Components
    private VehiclePhysics _vehiclePhysics;
    public VehiclePhysics VehiclePhysics
    {
        get
        {
            if (_vehiclePhysics == null)
            {
                _vehiclePhysics = GetComponent<VehiclePhysics>();
            }
            return _vehiclePhysics;
        }
    }
    #endregion
}
