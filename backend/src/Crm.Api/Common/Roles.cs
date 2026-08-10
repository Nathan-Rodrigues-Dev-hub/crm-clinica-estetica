namespace Crm.Api.Common;

/// <summary>Nomes de papéis para [Authorize(Roles = ...)] — casam com o enum Papel.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Profissional = "Profissional";
    public const string Recepcao = "Recepcao";
    /// <summary>Suporte/dono da plataforma — tem tudo que Admin tem, mais o exclusivo de gerenciar Profissionais.</summary>
    public const string SuperAdmin = "SuperAdmin";

    public const string AdminOuSuperAdmin = Admin + "," + SuperAdmin;
    public const string AdminOuRecepcao = Admin + "," + Recepcao + "," + SuperAdmin;
    public const string AdminOuProfissional = Admin + "," + Profissional + "," + SuperAdmin;
    public const string Interno = Admin + "," + Profissional + "," + Recepcao + "," + SuperAdmin;
}
