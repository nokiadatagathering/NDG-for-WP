/* 
    Copyright (C) 2011  Comarch
  
    NDG for WP7 is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either 
    version 2.1 of the License, or (at your option) any later version.
  
    NDG is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
  
    You should have received a copy of the GNU Lesser General Public
    License along with NDG.  If not, see <http://www.gnu.org/licenses/
*/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class responsible for data encryption.
    /// </summary>
     public class AESEncryption
     {
         /// <summary>
         /// Encrypts string data. Uses specific password as a key.
         /// </summary>
         /// <param name="dataToEncrypt">Data you want to encrypt.</param>
         /// <param name="password">Password used as a key to encryption.</param>
         /// <param name="salt">String used to generate key based on password. Must be at least 8 bytes long.</param>
         /// <returns>Encrypted string data.</returns>
         public string Encrypt(string dataToEncrypt, string password, string salt)
         {
             AesManaged aes = null;
             MemoryStream memoryStream = null;
             CryptoStream cryptoStream = null;
             try
             {
                 // Generate a Key based on a Password and HMACSHA1 pseudo-random number generator
                 // Salt must be at least 8 bytes long
                 // Use an iteration count of at least 1000
                 Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);

                 // Create AES algorithm
                 aes = new AesManaged();

                 // Key derived from byte array with 32 pseudo-random key bytes
                 aes.Key = rfc2898.GetBytes(32);

                 // IV derived from byte array with 16 pseudo-random key bytes
                 aes.IV = rfc2898.GetBytes(16);

                 // Create Memory and Crypto Streams
                 memoryStream = new MemoryStream();
                 cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                 // Encrypt Data
                 byte[] data = Encoding.UTF8.GetBytes(dataToEncrypt);
                 cryptoStream.Write(data, 0, data.Length);
                 cryptoStream.FlushFinalBlock();

                 // Return Base 64 String
                 return Convert.ToBase64String(memoryStream.ToArray());
             }
             finally
             {
                 if (cryptoStream != null)
                 {
                     cryptoStream.Close();
                 }

                 if (memoryStream != null)
                 {
                     memoryStream.Close();
                 }

                 if (aes != null)
                 {
                     aes.Clear();
                 }
             }
         }

         /// <summary>
         /// Decrypts string data. Uses specific password as a key.
         /// </summary>
         /// <param name="dataToDecrypt">Encrypted data.</param>
         /// <param name="password">Password used as a key to decryption.</param>
         /// <param name="salt">String used to generate key based on password. Must be at least 8 bytes long.</param>
         /// <returns>Decrypted string data.</returns>
         public string Decrypt(string dataToDecrypt, string password, string salt)
         {
             AesManaged aes = null;
             MemoryStream memoryStream = null;

             try
             {
                 // Generate a Key based on a Password and HMACSHA1 pseudo-random number generator
                 // Salt must be at least 8 bytes long
                 // Use an iteration count of at least 1000
                 Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);

                 // Create AES algorithm
                 aes = new AesManaged();

                 // Key derived from byte array with 32 pseudo-random key bytes
                 aes.Key = rfc2898.GetBytes(32);

                 // IV derived from byte array with 16 pseudo-random key bytes
                 aes.IV = rfc2898.GetBytes(16);

                 // Create Memory and Crypto Streams
                 memoryStream = new MemoryStream();
                 CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

                 // Decrypt Data
                 byte[] data = Convert.FromBase64String(dataToDecrypt);
                 cryptoStream.Write(data, 0, data.Length);
                 cryptoStream.FlushFinalBlock();

                 // Return Decrypted String
                 byte[] decryptBytes = memoryStream.ToArray();

                 // Dispose
                 if (cryptoStream != null)
                 {
                     cryptoStream.Dispose();
                 }

                 // Retval
                 return Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
             }
             finally
             {
                 if (memoryStream != null)
                 {
                     memoryStream.Dispose();
                 }

                 if (aes != null)
                 {
                     aes.Clear();
                 }
             }
         }
     }
 }