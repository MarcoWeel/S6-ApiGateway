namespace API_Gateway.Models;

[Flags]
public enum Roles
{
    User = 1,
    ShopManager = 2,
    Admin = 4,
}
