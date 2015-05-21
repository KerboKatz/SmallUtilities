using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public partial class ModifiedExplosionPotential : KerboKatzBase
  {
    private Dictionary<string, double> resourceExplosionModifiers = new Dictionary<string, double>();
    private float baseExplosiveness;
    private float nextUpdate;
    private float updateInterval;
    public ModifiedExplosionPotential()
    {
      modName = "SmallUtilities.ModifiedExplosionPotential";
      requiresUtilities = new Version(1, 2, 5);
    }

    protected override void Started()
    {
      currentSettings.load("SmallUtilities", "ModifiedExplosionPotential", modName);
      currentSettings.setDefault("baseExplosiveness", "0.1");
      currentSettings.setDefault("updateInterval", "1.00");
      baseExplosiveness = currentSettings.getFloat("baseExplosiveness");
      updateInterval = currentSettings.getFloat("updateInterval");

      foreach (var resource in PartResourceLibrary.Instance.resourceDefinitions)
      {
        if (currentSettings.isSet(resource.name))
        {
          resourceExplosionModifiers.Add(resource.name, currentSettings.getDouble(resource.name));
          continue;
        }
        if (!resourceExplosionModifiers.ContainsKey(resource.name))
        {
          resourceExplosionModifiers.Add(resource.name, resource.density);
          currentSettings.set(resource.name, resource.density);
        }
      }
      GameEvents.onVesselLoaded.Add(onVesselLoad);
    }

    private void FixedUpdate()
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
        nextUpdate = Time.time + updateInterval;
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
      double explosiveness = baseExplosiveness;
      foreach (var resource in part.Resources.list)
      {
        if (resourceExplosionModifiers.ContainsKey(resource.resourceName))
        {
          explosiveness += resource.amount * resourceExplosionModifiers[resource.resourceName];
        }
      }
      part.explosionPotential = (float)explosiveness;
    }

    protected override void afterDestroy()
    {
      GameEvents.onVesselLoaded.Remove(onVesselLoad);
    }
  }
}