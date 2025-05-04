using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel.Data
{
    public static class Verification
    {
        private const int SaltSize = 16; // 128 бит
        private const int HashSize = 20; // 160 бит
        private const int Iterations = 10000; // Количество итераций PBKDF2
        private const int MaxPasswordLength = 255; // Максимальная длина в БД

        public static string HashPassword(string password)
        {
            // Генерация соли
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // Создание хеша
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            // Комбинирование соли и хеша
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Конвертация в base64
            var base64Hash = Convert.ToBase64String(hashBytes);

            // Проверка на максимальную длину
            if (base64Hash.Length > MaxPasswordLength)
                throw new Exception("Хеш пароля превышает максимальную длину");

            return base64Hash;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Конвертация из base64
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // Извлечение соли
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Создание хеша введенного пароля
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            // Сравнение хешей
            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
