namespace FUNews.Presentation.Constants
{
    public static class AccountRoles
    {
        public const int Admin = 3;
        public const int Lecturer = 2;
        public const int Staff = 1;

        public static string GetRoleByRoleId(int? roleId)
        {
            switch (roleId)
            {
                case Admin:
                    return "Admin";
                case Lecturer:
                    return "Lecturer";
                case Staff:
                    return "Staff";
                default:
                    return "Unknown";
            }
        }
    }
}
