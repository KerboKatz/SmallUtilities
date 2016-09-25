using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerboKatz.MEP
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public partial class ModifiedExplosionPotential : KerboKatzBase<Settings>
  {
    private float nextUpdate;

    public ModifiedExplosionPotential()
    {
      modName = "ModifiedExplosionPotential";
      displayName = "Modified Explosion Potential";
      requiresUtilities = new Version(1, 4, 0);
      Log("Init done!");
    }

    public override void OnAwake()
    {
      LoadSettings("SmallUtilities/ModifiedExplosionPotential", "Settings");

      foreach (var resource in PartResourceLibrary.Instance.resourceDefinitions)
      {
        if (!settings.IsExplosionValueSet(resource.name))
        {
          settings.AddExplosionValue(resource.name, resource.density);
          continue;
        }
      }
      GameEvents.onVesselLoaded.Add(onVesselLoad);
      GameEvents.onPartUnpack.Add(onPartUnpack);
    }

    private void onPartUnpack(Part part)
    {
      updateExplosionPotential(part);
    }

    private void Update()
    {
      if (nextUpdate < Time.time)
      {
        List<Vessel> vessels = new List<Vessel>();
        foreach (var vessel in FlightGlobals.Vessels)
        {
          if (vessel.loaded)
          {
            foreach (var part in vessel.parts)
            {
              if (part.Resources.Count == 0)
                continue;
              updateExplosionPotential(part);
            }
          }
        }
        nextUpdate = Time.time + settings.updateInterval;
      }
    }

    private void onVesselLoad(Vessel vessel)
    {
      foreach (var part in vessel.parts)
      {
        updateExplosionPotential(part);
      }
    }

    private void updateExplosionPotential(Part part)
    {
      double explosiveness = settings.baseExplosiveness;
      float currentExplosiveness;
      foreach (var resource in part.Resources)
      {
        if (settings.GetExplosiveness(resource.resourceName, out currentExplosiveness))
        {
          explosiveness += resource.amount * currentExplosiveness;
        }
      }
      part.explosionPotential = (float)explosiveness;
    }

    protected override void AfterDestroy()
    {
      GameEvents.onPartUnpack.Remove(onPartUnpack);
      GameEvents.onVesselLoaded.Remove(onVesselLoad);
    }
  }
}