namespace SuperPasses.Models;

public class Config
{
    public string RouterIp { get; set; } = "192.168.101.1";
    public string RouterPort { get; set; } = "22";
    public string RouterAccount { get; set; } = "root";
    public string RouterPassword { get; set; } = "";
    public string SchoolAccount { get; set; } = "";
    public string SchoolPassword { get; set; } = "";
}