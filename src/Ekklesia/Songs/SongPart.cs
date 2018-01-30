using System;
using System.Collections.Generic;
using System.Text;

namespace Ekklesia.Songs
{
    internal class SongPart
    {
        public string Identifier { get; set; }
        public List<string> Lines { get; set; }

        public override string ToString()
        {
            if (Identifier == null && Lines == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"[{Identifier}]");
            builder.Append(string.Join(Environment.NewLine, Lines));

            return builder.ToString();
        }
    }
}
