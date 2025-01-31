public class FiatCommand
{
  public static readonly FiatCommand DEEPREFRESH = new() { Action = "ev", Message = "DEEPREFRESH" }; // EVs only - Deep Refresh Statistics
  public static readonly FiatCommand VF = new() { Action = "location", Message = "VF" }; // Update GPS
  public static readonly FiatCommand HBLF = new() { Message = "HBLF" }; // Theft Alarm - Lights+Horn
  public static readonly FiatCommand REON = new() { Message = "REON" }; // Engine On
  public static readonly FiatCommand REOFF = new() { Message = "REOFF" }; // Engine Off
  public static readonly FiatCommand TA = new() { Message = "TA" }; // Theft Alarm Surpress
  public static readonly FiatCommand ROTRUNKLOCK = new() { Message = "ROTRUNKLOCK" }; // Lock Trunk
  public static readonly FiatCommand ROTRUNKUNLOCK = new() { Message = "ROTRUNKUNLOCK" }; // Unlock Trunk
  public static readonly FiatCommand ROLIFTGATELOCK = new() { Message = "ROLIFTGATELOCK" }; // Lock Liftgate
  public static readonly FiatCommand ROLIFTGATEUNLOCK = new() { Message = "ROLIFTGATEUNLOCK" }; // Unlock Liftgate  
  public static readonly FiatCommand ROCOMFORTON = new() { Message = "ROCOMFORTON" }; // Comfort On
  public static readonly FiatCommand ROCOMFORTOFF = new() { Message = "ROCOMFORTOFF" }; // Comfort Off
  public static readonly FiatCommand ROPRECOND = new() { Message = "ROPRECOND" }; // Preconditioning On
  public static readonly FiatCommand ROPRECOND_OFF = new() { Message = "ROPRECOND_OFF" }; // Preconditioning Off
  public static readonly FiatCommand ROHVACON = new() { Message = "ROHVACON" }; // HVAC On
  public static readonly FiatCommand ROHVACOFF = new() { Message = "ROHVACOFF" }; // HVAC Off
  public static readonly FiatCommand CNOW = new() { Action = "ev/chargenow", Message = "CNOW" }; // EVs only - Charge Now
  public static readonly FiatCommand RDU = new() { Message = "RDU" }; // Unlock Doors
  public static readonly FiatCommand RDL = new() { Message = "RDL" }; // Lock Doors
  public static readonly FiatCommand ROLIGHTS = new() { Message = "ROLIGHTS" }; // Lights
  
  public required string Message { get; init; }
  public string Action { get; init; } = "remote";
}
