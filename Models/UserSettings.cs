
namespace Models.UserSettings;
class UserSettings
{
    public bool FileTypeLua {  get; set; }
    public string? FileNameLua { get; set; }
    public string? PathLua {  get; set; }

    public bool FileTypeJson { get; set; } 
    public string? FileNameJson { get; set; }   
    public string? PathJson {  get; set; }

    public int Interval { get; set; }
    public bool SaveSettings { get; set; }

    public bool LoadSettings { get; set; }
}