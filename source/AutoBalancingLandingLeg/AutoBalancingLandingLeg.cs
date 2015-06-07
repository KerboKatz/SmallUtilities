using System.Text;
using UnityEngine;

namespace KerboKatz
{
  public class ModuleAutoBalancingLandingLeg : PartModule
  {
    [KSPField(isPersistant = false)]
    public string animationName;

    [KSPField(isPersistant = false)]
    public string wheelColliderName;

    [KSPField(isPersistant = false)]
    public string suspensionTransformName;

    [KSPField(isPersistant = false)]
    public string landingFootName;

    [KSPField(isPersistant = false)]
    public bool orientFootToGround;

    [KSPField(isPersistant = false)]
    public bool alignFootUp;

    [KSPField(isPersistant = true)]
    public bool raised = true;

    [KSPField(isPersistant = false)]
    public bool changeDamper = true;

    [KSPField(isPersistant = false)]
    public bool changeSuspension = true;

    [KSPField(isPersistant = true)]
    public bool lockSuspensionSpring = false;

    [KSPField(isPersistant = false)]
    public int breakFrames = 2;

    [KSPField(isPersistant = false)]
    public float suspensionUpperLimit;

    [KSPField(isPersistant = false)]
    public float impactTolerance;

    [KSPField(isPersistant = false)]
    public float precision = 0.015f;

    [KSPField(isPersistant = false)]
    public float maxSuspension = 10;

    [KSPField(isPersistant = false)]
    public float minSuspension = 0.05f;

    [KSPField(isPersistant = false)]
    public float maxDamper = 10;

    [KSPField(isPersistant = false)]
    public float minDamper = 0.05f;

    [KSPField(isPersistant = false)]
    public float suspensionSpring;

    [KSPField(isPersistant = false)]
    public float steps = 2.5f;

    [KSPField(isPersistant = false)]
    public float suspensionDamper;

    [KSPField(isPersistant = true)]
    public float suspensionSpringReal;

    [KSPField(isPersistant = true)]
    public float suspensionDamperReal;

    [KSPField(isPersistant = false)]
    public Vector3 suspensionOffset;

    [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
    public LegStates legState;

    private Animation legAnimation;
    private AnimationState legAnimationState;

    public WheelCollider wheelCollider;
    private Transform suspensionTransform;
    private WheelHit hit;
    private bool grounded;
    private bool readyForDisable = true;

    private int count = 0;
    private Transform landingFoot;

    public enum LegStates
    {
      Retracted = 0,
      Retracting = 1,
      Deploying = 2,
      Deployed = 3,
      Broken = 4,
      Repairing = 5,
    };

    public override void OnAwake()
    {
      suspensionSpringReal = suspensionSpring;
      suspensionDamperReal = suspensionDamper;
    }

    public override void OnInitialize()
    {
      legAnimation                = part.FindModelAnimators()[0];
      legAnimationState           = legAnimation[animationName];
      wheelCollider               = part.FindModelTransform(wheelColliderName).GetComponent<WheelCollider>();
      suspensionTransform         = part.FindModelTransform(suspensionTransformName);
      landingFoot                 = part.FindModelTransform(landingFootName);
      if (!raised)
      {
        legAnimationState.time = legAnimationState.length;
        legAnimationState.speed = 1;
        legAnimation.Play(animationName);
        wheelCollider.enabled = true;
      }
      updateSpring();
    }

    private void updateSpring()
    {
      JointSpring jointSpring = new JointSpring();
      jointSpring.spring = suspensionSpringReal;
      jointSpring.damper = suspensionDamperReal;
      if (wheelCollider != null)
      {
        wheelCollider.suspensionSpring = jointSpring;
      }
    }

    private void adjustSpring()
    {
      if (lockSuspensionSpring)
        return;
      if (!grounded)
        return;
      if (!readyForDisable)
      {//put in a little break so the vessel can balance itself out.
        if (count == breakFrames)
        {
          readyForDisable = true;
          count = 0;
          return;
        }
        count++;
        return;
      }
      if (!readyForDisable)
        return;

      double combinedDistance = 0;
      int partCount = 0;
      foreach (var landingGear in vessel.FindPartModulesImplementing<ModuleAutoBalancingLandingLeg>())
      {
        if (!landingGear.grounded || !landingGear.wheelCollider.enabled || legState == LegStates.Broken)
          continue;
        combinedDistance += Vector3d.Distance(vessel.mainBody.position, landingGear.part.transform.position);
        partCount++;
      }
      double averageDistance = combinedDistance / partCount;
      double thisDistance = Vector3d.Distance(vessel.mainBody.position, part.transform.position);
      var distance = (float)(thisDistance - averageDistance);
      if (!isDifferenceHighEnough(distance))
        return;

      if (!modifySuspension(distance))
        return;
      suspensionSpringReal = Mathf.Clamp(suspensionSpringReal, minSuspension, maxSuspension);
      suspensionDamperReal = Mathf.Clamp(suspensionDamper, minDamper, maxDamper);
      readyForDisable = false;
      wheelCollider.enabled = false;
      wheelCollider.enabled = true;
      updateSpring();
    }

    private bool modifySuspension(double modifier)
    {
      if (modifier < 0)
      {
        if (suspensionSpringReal >= maxSuspension)
          if (!modifyDamper(modifier))
            return false;
      }
      else
      {
        if (suspensionSpringReal <= minSuspension)
          if (!modifyDamper(modifier))
            return false;
      }
      suspensionSpringReal -= steps * (float)modifier;
      return true;
    }

    private bool modifyDamper(double modifier)
    {
      if (!changeDamper)
        return false;
      if (modifier < 0)
      {
        if (suspensionDamperReal >= maxDamper)
          return false;
      }
      else
      {
        if (suspensionDamperReal <= minDamper)
          return false;
      }
      suspensionDamperReal += steps * (float)modifier;
      return true;
    }

    private bool isDifferenceHighEnough(double diff)
    {
      if (diff == 0)
        return false;
      if (diff > 0)
      {
        if (diff < precision)
          return false;
      }
      else
      {
        if (diff > -precision)
          return false;
      }
      return true;
    }

    public void FixedUpdate()
    {
      if (!HighLogic.LoadedSceneIsFlight)
        return;
      if (!FlightGlobals.ready)
        return;
      if (legAnimation == null)
        return;
      if (wheelCollider == null)
        return;
      if (legState == LegStates.Broken)
        return;
      if (legAnimation.isPlaying)
        return;
      if (raised && wheelCollider.enabled == true)
      {
        wheelCollider.enabled = false;
      }
      else if (!raised && wheelCollider.enabled == false)
      {
        wheelCollider.enabled = true;
      }
      if (raised)
        return;
      if (wheelCollider.enabled == false)
        return;
      updateSuspension();
      adjustSpring();
      increaseBrakeTorque();
    }

    private void increaseBrakeTorque()
    {
      if (wheelCollider.brakeTorque < impactTolerance && wheelCollider.enabled)
        wheelCollider.brakeTorque += 10;
    }

    private void updateSuspension()
    {
      if (vessel.Landed && wheelCollider.GetGroundHit(out hit))
      {
        grounded = true;
        if (!lockSuspensionSpring)
        {
          var hitPosition = wheelCollider.transform.InverseTransformPoint(hit.point);
          var positionUP = wheelCollider.transform.InverseTransformPoint(wheelCollider.transform.position + wheelCollider.transform.up);
          var inversePosition = (hitPosition / part.rescaleFactor + positionUP) - suspensionOffset;
          if (inversePosition.y > suspensionUpperLimit)
          {
            inversePosition.y = suspensionUpperLimit;
          }
          suspensionTransform.position = wheelCollider.transform.TransformPoint(inversePosition);
        }
        if (hit.force > impactTolerance)
        {
          legState = LegStates.Broken;
          wheelCollider.enabled = false;
        }
        if (orientFootToGround)
        {
          if (alignFootUp)
            landingFoot.up = -hit.normal;
          else
            landingFoot.forward.Set(landingFoot.forward.x, -hit.normal.y, landingFoot.forward.z);
        }
      }
      else
      {
        if (!vessel.Landed && !lockSuspensionSpring)
        {
          if (suspensionSpringReal != suspensionSpring || suspensionDamperReal != suspensionDamper)
          {
            suspensionSpringReal = suspensionSpring;
            suspensionDamperReal = suspensionDamper;
            updateSpring();
          }
        }
        grounded = false;
        if (!lockSuspensionSpring)
          suspensionTransform.position = Vector3.Lerp(suspensionTransform.position, wheelCollider.transform.position, Time.deltaTime);
      }
    }

    private void lowerLeg()
    {
      raised = false;
      legAnimationState.speed = 1;
      legAnimation.Play(animationName);
      wheelCollider.brakeTorque = 0;
      setDragCube(0);
      legState = LegStates.Deployed;
    }

    private void raiseLeg()
    {
      legState = LegStates.Retracted;
      wheelCollider.enabled = false;
      raised = true;
      legAnimationState.speed = -1;
      if (!legAnimation.isPlaying)
      {
        legAnimationState.time = legAnimationState.length;
      }
      legAnimation.Play(animationName);

      setDragCube(1);
    }

    private void setDragCube(float value)
    {
      part.DragCubes.SetCubeWeight("DEPLOYED", 1 - value);
      part.DragCubes.SetCubeWeight("RETRACTED", value);
    }

    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Toggle Leg")]
    public void toggleLeg()
    {
      if (legState == LegStates.Broken)
        return;
      if (raised)
      {
        lowerLeg();
      }
      else
      {
        raiseLeg();
      }
    }

    [KSPAction("Raise leg")]
    public void raiseLeg(KSPActionParam param)
    {
      if (legState == LegStates.Broken)
        return;
      if (raised)
        return;
      raiseLeg();
    }

    [KSPAction("Lower leg")]
    public void lowerLeg(KSPActionParam param)
    {
      if (legState == LegStates.Broken)
        return;
      if (!raised)
        return;
      lowerLeg();
    }

    [KSPAction("Toggle Leg", KSPActionGroup.Gear)]
    public void toggleLeg(KSPActionParam param)
    {
      toggleLeg();
    }

    [KSPAction("Toggle spring")]
    public void toggleSpringLock(KSPActionParam param)
    {
      if (lockSuspensionSpring)
      {
        unlockSpring();
      }
      else
      {
        lockSpring();
      }
    }

    [KSPAction("Lock spring")]
    public void lockSpring(KSPActionParam param)
    {
      lockSpring();
    }

    [KSPAction("Unlock spring")]
    public void unlockSpring(KSPActionParam param)
    {
      unlockSpring();
    }

    [KSPEvent(guiActive = true, guiName = "Lock spring")]
    public void lockSpring()
    {
      lockSuspensionSpring = true;
      Events["lockSpring"].guiActive = false;
      Events["unlockSpring"].guiActive = true;
      updateSpring();
    }

    [KSPEvent(guiActive = false, guiName = "Unlock spring")]
    public void unlockSpring()
    {
      lockSuspensionSpring = false;
      Events["lockSpring"].guiActive = true;
      Events["unlockSpring"].guiActive = false;
      updateSpring();
    }

    [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Toggle Leg")]
    public void toggleLegEditor()
    {
      toggleLeg();
      foreach (var symmetryCounterpart in part.symmetryCounterparts)
      {
        foreach (var symmetryCounterpartGear in symmetryCounterpart.FindModulesImplementing<ModuleAutoBalancingLandingLeg>())
        {
          symmetryCounterpartGear.toggleLeg();
        }
      }
    }

    [KSPEvent(guiName = "Repair Leg", guiActiveEditor = false, guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 4, guiActive = false)]
    public void repairLeg()
    {
      if (legState != LegStates.Broken)
        return;
      if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < 3)
      {
        ScreenMessages.PostScreenMessage("Engineering skill too low! Need level 3 to repair landing legs. Current level: " + FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value);
        return;
      }
      legState = LegStates.Deployed;
      ScreenMessages.PostScreenMessage("Repaired " + part.partInfo.name);
    }

    public override string GetInfo()
    {
      var builder = new StringBuilder();
      builder.AppendLine("Auto balancing landing Leg");
      builder.AppendLine("By KerboKatz");
      builder.AppendLine("Align foot up: " + alignFootUp);
      builder.AppendLine("Change damper: " + changeDamper);
      builder.AppendLine("Change suspension spring: " + changeSuspension);
      builder.AppendLine("Suspension upper limit: " + suspensionUpperLimit);
      builder.AppendLine("Impact tolerance: " + impactTolerance);
      if (changeSuspension)
      {
        builder.AppendLine("Min. suspension: " + minSuspension);
        builder.AppendLine("Default suspension: " + suspensionSpring);
        builder.AppendLine("Max. suspension: " + maxSuspension);
      }
      if (changeDamper)
      {
        builder.AppendLine("Min. damper: " + minDamper);
        builder.AppendLine("Default damper: " + suspensionDamper);
        builder.AppendLine("Max. damper: " + maxDamper);
      }
      return builder.ToString();
    }
  }
}