namespace KerboKatz.KSX
{
  public class Settings : SettingsBase<Settings>
  {
    public float repDelta = 10;
    public float repHigh = 10000;
    public float repLow = 0;
    internal bool showExchange;
    public float tax = 1;
    public float ratio = 10000;
  }
}