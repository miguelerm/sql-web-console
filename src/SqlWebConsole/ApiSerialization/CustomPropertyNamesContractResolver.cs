using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlWebConsole.ApiSerialization
{
    public class CustomPropertyNamesContractResolver : DefaultContractResolver
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        protected override string ResolvePropertyName(string propertyName)
        {
            return ChangeCase(propertyName);
        }

        private string ChangeCase(string s)
        {
            var sb = new StringBuilder(s.Length);

            bool isNextUpper = false, isPrevLower = false;
            foreach (var c in s)
            {
                if (c == '_')
                {
                    isNextUpper = true;
                }
                else
                {
                    sb.Append(isNextUpper ? char.ToUpper(c, Culture) : isPrevLower ? c : char.ToLower(c, Culture));
                    isNextUpper = false;
                    isPrevLower = char.IsLower(c);
                }
            }
            return sb.ToString();
        }
    }
}
