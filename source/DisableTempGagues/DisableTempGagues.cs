using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public partial class DisableTempGagues : MonoBehaviour
  {
    private bool setToFalse;
    public void Update()
    {
      if (!setToFalse)
      {
        if (TemperatureGagueSystem.Instance != null)
        {
          TemperatureGagueSystem.Instance.showGagues = false;
          setToFalse = true;
        }
      }
    }
  }
}