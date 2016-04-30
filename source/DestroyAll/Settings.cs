using KerboKatz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerboKatz.DA
{
  public class Settings : SettingsBase<Settings>
  {
    public bool hideRecoveryDialog = true;
    public bool showActiveVessels;
    public bool showSettings;
    public List<VesselToggle> vesselTypeToggles = new List<VesselToggle>();
    public bool includePrelaunch;

    public class VesselToggle
    {
      public VesselType type;
      public bool toggle;
    }

    public VesselToggle GetType(VesselType type)
    {
      foreach (var vesselToggle in vesselTypeToggles)
      {
        if (vesselToggle.type == type)
        {
          return vesselToggle;
        }
      }
      var newVesselToggle = new VesselToggle();
      newVesselToggle.type = type;
      vesselTypeToggles.Add(newVesselToggle);
      return newVesselToggle;
    }
  }
}
