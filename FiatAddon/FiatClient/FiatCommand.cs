public class FCACommand
{
  public static readonly FCACommand DEEPREFRESH = new() { Action = "ev", Message = "DEEPREFRESH" };
  public static readonly FCACommand VF = new() { Action = "location", Message = "VF" };
  public static readonly FCACommand HBLF = new() { Message = "HBLF" };
  public static readonly FCACommand REON = new() { Message = "REON" };
  public static readonly FCACommand REOFF = new() { Message = "REOFF" };
  public static readonly FCACommand TA = new() { Message = "TA" };
  public static readonly FCACommand ROTRUNKLOCK = new() { Message = "ROTRUNKLOCK" };
  public static readonly FCACommand ROTRUNKUNLOCK = new() { Message = "ROTRUNKUNLOCK" };
  public static readonly FCACommand ROPRECOND = new() { Message = "ROPRECOND" };
  public static readonly FCACommand RDU = new() { Message = "RDU" };
  public static readonly FCACommand RDL = new() { Message = "RDL" };
  public required string Message { get; init; }
  public string Action { get; init; } = "remote";
}
