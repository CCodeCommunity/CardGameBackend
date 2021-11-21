using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace Api.Utilities;

public static class Argon2Utils
{
    private static byte[] CreateSalt()
    {
        var buffer = new byte[16];
        var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(buffer);
        return buffer;
    }
        
    public static async Task<string> HashPasswordAsync(string password)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = CreateSalt();
        argon2.DegreeOfParallelism = 8; // four cores
        argon2.Iterations = 4;
        argon2.MemorySize = 1024 * 1024; // 1 GB

        var bytes = await argon2.GetBytesAsync(16); 
        
        return bytes.ToString()!;
    }
    
    public static string HashPassword(string password)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = CreateSalt();
        argon2.DegreeOfParallelism = 8; // four cores
        argon2.Iterations = 4;
        argon2.MemorySize = 1024 * 1024; // 1 GB

        var bytes = argon2.GetBytes(16); 
        
        return bytes.ToString()!;
    }
        
    public static async Task<bool> VerifyHashAsync(string password, string hash)
    {
        var newHash = await HashPasswordAsync(password);
        return hash == newHash;
    }
}