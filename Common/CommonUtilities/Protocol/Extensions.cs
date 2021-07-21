using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Protocol
{
    internal static class Extensions
    {
        public static async Task<byte> ReadByteAsync(this Stream s, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[1];

            await s.ReadAsync(buffer.AsMemory(), cancellationToken);

            return buffer[0];
        }
    }
}
