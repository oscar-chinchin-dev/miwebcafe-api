namespace MiWebCafe.API.Configuration;

public sealed class InitialUsersOptions
{
    public const string SectionName = "InitialUsers";

    public InitialUserOptions Admin { get; init; } = new();
    public InitialUserOptions Cajero { get; init; } = new();

    public bool IsValid() => Admin.IsValid() && Cajero.IsValid();
}

public sealed class InitialUserOptions
{
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Nombre) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password);
}
