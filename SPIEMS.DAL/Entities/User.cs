namespace SPIEMS.DAL.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    // ✅ safest default
    public string Role { get; set; } = "Customer";
}
