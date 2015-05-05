using System.Collections.Generic;

namespace KerboKatz
{
  public partial class Afterburner : PartModule
  {
    [KSPField]
    public float thrustBonus;
    [KSPField]
    public float fuelEfficencyDecrease;
    [KSPField]
    public float heatIncrease;
    [KSPField(isPersistant = true)]
    public bool burning;
    [KSPField]
    public string engineID;
    private Dictionary<string, atmosphereCurves> ModuleEnginesCurves = new Dictionary<string, atmosphereCurves>();
    private float realFuelEfficencyDecrease;
    private float realHeatIncrease;
    private float realThrustBonus;
    private List<ModuleEngines> ModuleEngines;
    private string info;

    public override void OnAwake()
    {
      realFuelEfficencyDecrease = (fuelEfficencyDecrease / 100) + 1;
      realThrustBonus = ((thrustBonus / 100) + 1) * realFuelEfficencyDecrease;
      realHeatIncrease = ((heatIncrease / 100) + 1) * realThrustBonus;

      ModuleEngines = part.FindModulesImplementing<ModuleEngines>();
      var toBeRemoved = new List<ModuleEngines>();
      foreach (var moduleEngine in ModuleEngines)
      {
        if (moduleEngine.engineID == engineID || engineID.IsNullOrWhiteSpace())
        {
          if (!ModuleEnginesCurves.ContainsKey(moduleEngine.engineID))
          {
            ModuleEnginesCurves.Add(moduleEngine.engineID, new atmosphereCurves(moduleEngine.atmosphereCurve, realFuelEfficencyDecrease));
          }
        }
        else
        {
          toBeRemoved.Add(moduleEngine);
        }
      }
      foreach (var remove in toBeRemoved)
      {
        ModuleEngines.Remove(remove);
      }
    }

    public override void OnLoad(ConfigNode node)
    {
      if (node.HasValue("burning"))
        bool.TryParse(node.GetValue("burning"), out burning);
      if (burning)
      {
        startBurning();
      }
    }

    public override string GetInfo()
    {
      if (info == null)
      {
        info = "Includes KerboKatz Afterburner\n";
        info += "Thrust boost: " + thrustBonus + "%\n";
        info += "Inefficiency: " + fuelEfficencyDecrease + "%\n";
        info += "Heat increase: " + heatIncrease + "%";
      }
      return info;
    }

    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Turn Afterburner on")]
    public void ToggleAfterburner()
    {
      if (!burning)
      {
        startBurning();
        burning = true;
      }
      else
      {
        stopBurning();
        burning = false;
      }
    }
    [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Turn Afterburner on")]
    public void ToggleAfterburnerEditor()
    {
      if (HighLogic.LoadedSceneIsEditor)
      {//Apply the same action to all counterparts
        //doing this the "complicated way" since i didn't find anything that makes this easier
        foreach (var symmetryCounterpart in part.symmetryCounterparts)
        {
          foreach (var symmetryCounterpartAfterburner in symmetryCounterpart.FindModulesImplementing<Afterburner>())
          {
            if (symmetryCounterpartAfterburner.engineID == engineID)
            {
              symmetryCounterpartAfterburner.ToggleAfterburner();
              symmetryCounterpartAfterburner.Events["ToggleAfterburnerEditor"].guiName = symmetryCounterpartAfterburner.Events["ToggleAfterburner"].guiName;
            }
          }
        }
        ToggleAfterburner();
        Events["ToggleAfterburnerEditor"].guiName = Events["ToggleAfterburner"].guiName;
      }
    }
    [KSPAction("Toggle Afterburner")]
    public void ToggleAfterburner(KSPActionParam param)
    {
      ToggleAfterburner();
    }

    private void stopBurning()
    {
      Events["ToggleAfterburner"].guiName = "Turn Afterburner on";
      foreach (var ModuleEngine in ModuleEngines)
      {
        ModuleEngine.atmosphereCurve = ModuleEnginesCurves[ModuleEngine.engineID].realAtmosphereCurve;
        ModuleEngine.maxFuelFlow = ModuleEngine.maxFuelFlow / realThrustBonus;
        ModuleEngine.maxThrust = ModuleEngine.maxThrust * realHeatIncrease;
      }
    }

    private void startBurning()
    {
      Events["ToggleAfterburner"].guiName = "Turn Afterburner off";
      foreach (var ModuleEngine in ModuleEngines)
      {
        ModuleEngine.atmosphereCurve = ModuleEnginesCurves[ModuleEngine.engineID].atmosphereCurve;
        ModuleEngine.maxFuelFlow = ModuleEngine.maxFuelFlow * realThrustBonus;
        ModuleEngine.maxThrust = ModuleEngine.maxThrust / realHeatIncrease;
      }
    }
  }

  public class atmosphereCurves
  {
    public FloatCurve atmosphereCurve;
    public FloatCurve realAtmosphereCurve;
    public atmosphereCurves(FloatCurve realAtmosphereCurve, float realFuelEfficencyDecrease)
    {
      this.realAtmosphereCurve = realAtmosphereCurve;
      atmosphereCurve = new FloatCurve();
      foreach (var currentCurve in realAtmosphereCurve.Curve.keys)
      {
        atmosphereCurve.Curve.AddKey(currentCurve.time, currentCurve.value / realFuelEfficencyDecrease);
      }
    }
  }
}