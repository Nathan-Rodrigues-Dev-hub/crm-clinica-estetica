namespace Crm.Api.Common;

/// <summary>Nomes de papéis para [Authorize(Roles = ...)] — casam com o enum Papel.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Profissional = "Profissional";
    public const string Recepcao = "Recepcao";

    public const string AdminOuRecepcao = Admin + "," + Recepcao;
    public const string AdminOuProfissional = Admin + "," + Profissional;
    public const string Interno = Admin + "," + Profissional + "," + Recepcao;
}
