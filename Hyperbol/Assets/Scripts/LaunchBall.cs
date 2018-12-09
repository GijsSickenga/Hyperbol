using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchBall : MonoBehaviour
{
    private Hyperbol currentBall;

    public void PickUpBall(Hyperbol ball)
    {
        currentBall = ball;
    }
    
	public void ShootBall()
    {
        if(currentBall != null)
        {
            currentBall.ShootBall(GetComponent<VehicleControls>().Team);
            currentBall = null;
        }
    }
}
