using System.Security.Cryptography;
namespace BellasBoutique.Web.Services;
public static class CifradoService {
 public static string Hash(string p){var salt=RandomNumberGenerator.GetBytes(16);return "v1:"+Convert.ToBase64String(salt)+":"+Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(p,salt,100000,HashAlgorithmName.SHA256,32));}
 public static bool Verificar(string p,string h){try{var a=h.Split(':');return a.Length==3&&a[0]=="v1"&&CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(a[2]),Rfc2898DeriveBytes.Pbkdf2(p,Convert.FromBase64String(a[1]),100000,HashAlgorithmName.SHA256,32));}catch(FormatException){return false;}}
 public static bool Valida(string p)=>p.Length>=8&&p.Any(char.IsUpper)&&p.Any(char.IsLower)&&p.Any(char.IsDigit)&&p.Any(c=>!char.IsLetterOrDigit(c)&&!char.IsWhiteSpace(c));
 public static string Nueva()=>"Bb!"+Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant()+"7";
}