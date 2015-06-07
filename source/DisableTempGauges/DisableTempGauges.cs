using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class DisableTempGauges : MonoBehaviour
  {
    public void Update()
    {
      if (TemperatureGagueSystem.Instance == null)
        return;
      TemperatureGagueSystem.Instance.showGagues = false;
      Destroy(this);
    }
  }
}