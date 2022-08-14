using System;
using System.Collections.Generic;
using System.Text;

namespace Availity.Homework.Services.Extensions
{
    public static partial class Extensions
    {
        public static StringBuilder AppendWithPrefixComma(this StringBuilder sb, string value, bool withComma = true)
        {
            if (withComma)
            {
                sb.Append(",");
            }

            sb.Append(value);

            return sb;
        }
    }
}
