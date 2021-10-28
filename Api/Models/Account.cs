// Disable null checks here as they are not necessary because we know these will be initialized by EF
#pragma warning disable 8618

namespace Api.Models
{
    public class Account
    {
        public int Id;
        public string Email;
        public string Password;
        public bool isAdmin;
    }
}