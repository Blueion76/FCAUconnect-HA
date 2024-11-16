public class FiatCommand
{
  public static readonly FiatCommand DEEPREFRESH = new() { Action = "ev", Message = "DEEPREFRESH" };
  public static readonly FiatCommand VF = new() { Action = "location", Message = "VF" };
  public static readonly FiatCommand HBLF = new() { Message = "HBLF", IsDangerous = true };
  public static readonly FiatCommand REON = new() { Message = "REON", IsDangerous = true };
  public static readonly FiatCommand REOFF = new() { Message = "REOFF", IsDangerous = true };
  public static readonly FiatCommand TA = new() { Message = "TA" };
  public static readonly FiatCommand ROTRUNKLOCK = new() { Message = "ROTRUNKLOCK", IsDangerous = true };
  public static readonly FiatCommand ROTRUNKUNLOCK = new() { Message = "ROTRUNKUNLOCK", IsDangerous = true };
  public static readonly FiatCommand ROPRECOND = new() { Message = "ROPRECOND", IsDangerous = true };
  public static readonly FiatCommand RDU = new() { Message = "RDU", IsDangerous = true };
  public static readonly FiatCommand RDL = new() { Message = "RDL", IsDangerous = true };

  public bool IsDangerous { get; set; }
  public required string Message { get; init; }
  public string Action { get; init; } = "remote";
}
