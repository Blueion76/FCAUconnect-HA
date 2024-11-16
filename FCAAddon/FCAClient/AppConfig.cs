using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FCAUconnect;

public record AppConfig
{
  [Required(AllowEmptyStrings = false)] public string FCAUser { get; set; } = null!;
  [Required(AllowEmptyStrings = false)] public string FCAPw { get; set; } = null!;
  public string? FCAPin { get; set; }
  [Required(AllowEmptyStrings = false)] public string MqttServer { get; set; } = null!;
  [Range(1, 65536)] public int MqttPort { get; set; } = 1883;
  public string MqttUser { get; set; } = "";
  public string MqttPw { get; set; } = "";
  [Range(1, 1440)] public int RefreshInterval { get; set; } = 15;
  public string CarUnknownLocation { get; set; } = "away";
  [Required(AllowEmptyStrings = false)]
  public string SupervisorToken { get; set; } = null!;
  public FcaBrand Brand { get; set; }
  public FcaRegion Region { get; set; } = FcaRegion.Europe;
  public string HomeAssistantUrl { get; set; } = "http://supervisor/core";
  public int StartDelaySeconds { get; set; } = 1; 
  public bool AutoRefreshLocation { get; set; } = false;
  public bool AutoRefreshBattery { get; set; } = false;
  public bool ConvertKmToMiles { get; set; } = false;
  public bool DevMode { get; set; } = false;
  public bool UseFakeApi { get; set; } = false;
  public bool Debug { get; set; } = false;

  public string ToStringWithoutSecrets()
  {
    var user = this.FCAUser[0..2] + new string('*', this.FCAUser[2..].Length);

    var tmp = this with
    {
      FCAUser = user,
      SupervisorToken = new string('*', this.SupervisorToken.Length),
      FCAPw = new string('*', this.FCAPw.Length),
      MqttPw = new string('*', this.MqttPw.Length),
      FCAPin = new string('*', this.FCAPin?.Length ?? 0)
    };

    return JsonConvert.SerializeObject(tmp, Formatting.Indented, new StringEnumConverter());
  }
  
  public bool IsPinSet()
  {
    return !string.IsNullOrWhiteSpace(this.FCAPin);
  }
}
