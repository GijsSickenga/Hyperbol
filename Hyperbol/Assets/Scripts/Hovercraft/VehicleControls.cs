using System.Collections;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Controls the vehicle with the specified inputs.
/// </summary>
[RequireComponent(typeof(VehicleStats))]
public class VehicleControls : MonoBehaviour
{
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

    public struct ButtonCombo
    {
        public KeyCode keyboardInput;
        // public 
    }
}
