using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using ComercioElectronicoMvc.Data;
using Microsoft.AspNetCore.Http;

namespace ComercioElectronicoMvc
{
    public class Utilities
    {
        internal static string GetStringSha256Hash(string input)
        {
            if (String.IsNullOrEmpty(input))
                return String.Empty;

            using (var sha = new SHA256Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

    }

}
