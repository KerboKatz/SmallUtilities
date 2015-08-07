using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KerboKatz
{
  public class ModuleAutoBalancingLandingLegUpgrade : PartModule
  {
    [KSPField(isPersistant = false)]
    public string wheelColliderName;

    [KSPField(isPersistant = true)]
    public Vector3 offset = new Vector3();

    public double groundDistance;
    public WheelCollider wheelCollider;

    private float difference;
    private List<ModuleAutoBalancingLandingLegUpgrade> allVesselLegs;
    private double combinedDistance;
    private int partCount;
    private float resetTime;
    private bool isGrounded;

    public override void OnInitialize()
    {
      wheelCollider = part.FindModelTransform(wheelColliderName).GetComponent<WheelCollider>();
      updateVesselLegs(vessel);
      if (offset.y != 0)
        wheelCollider.transform.position -= offset.y * wheelCollider.transform.up;
      GameEvents.onVesselChange.Add(updateVesselLegs);
    }

    protected void OnDestroy()
    {
      GameEvents.onVesselChange.Remove(updateVesselLegs);
    }

    private void updateVesselLegs(Vessel data)
    {
      allVesselLegs = vessel.FindPartModulesImplementing<ModuleAutoBalancingLandingLegUpgrade>();
    }

    public void FixedUpdate()
    {
      if (!HighLogic.LoadedSceneIsFlight)
        return;
      if (!FlightGlobals.ready)
        return;
      if (wheelCollider == null)
        return;
      if (wheelCollider.enabled == false)
        return;
      if (!wheelCollider.isGrounded)
      {
        if (resetTime > Time.time)
        {
          if (offset.y != 0)
            addOffset(-offset.y);
          isGrounded = false;
        }
        else if (resetTime == 0)
        {
          resetTime = Time.time + 1;
        }
        return;
      }
      else if (resetTime != 0)
      {
        isGrounded = true;
        resetTime = 0;
      }
      groundDistance = getDistanceToCore(part.transform.position);
      combinedDistance = 0;
      partCount = 0;
      foreach (var landingGear in allVesselLegs)
      {
        if (!landingGear.isGrounded)
          continue;
        combinedDistance += landingGear.groundDistance;
        partCount++;
      }
      difference = (float)(combinedDistance / partCount - groundDistance);
      difference = difference * (1 + Mathf.Max(difference, -difference));

      addOffset(difference);
    }

    private void addOffset(float difference)
    {
      wheelCollider.transform.position += offset.y * wheelCollider.transform.up;
      offset.y = Mathf.Lerp(offset.y, Mathf.Clamp(offset.y + difference, -1, 0), 0.05f);
      wheelCollider.transform.position -= offset.y * wheelCollider.transform.up;
    }

    private double getDistanceToCore(Vector3 position)
    {
      return Vector3d.Distance(vessel.mainBody.position, position);
    }

    public override string GetInfo()
    {
      var builder = new StringBuilder();
      builder.AppendLine("Auto balancing landing Leg");
      builder.AppendLine("By KerboKatz");
      return builder.ToString();
    }
  }
}