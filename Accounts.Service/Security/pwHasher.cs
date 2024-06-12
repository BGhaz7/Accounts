using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Accounts.Service.Security
{
    public class pwHasher
    {
        public string HashPw(string pw)
        {
            //Randomization of the salt variable BEFORE hashing
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = RandomNumberGenerator.Create())
            {
                rngCsp.GetNonZeroBytes(salt);
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: pw,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Return the salt and the hash
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public bool VerifyHashedPw(string hashedPwWithSalt, string pw)
        {
            var parts = hashedPwWithSalt.Split(':', 2);

            if (parts.Length != 2)
            {
                throw new FormatException("The stored password is not in the expected format.");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hashed = Convert.FromBase64String(parts[1]);

            // Derive the hash from the given password and salt
            string verificationHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: pw,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed.SequenceEqual(Convert.FromBase64String(verificationHash));
        }
    }
}