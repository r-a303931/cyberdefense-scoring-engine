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
        private static readonly byte[] Buffer = new byte[1];

        public static async Task<byte> ReadByteAsync(this Stream s, CancellationToken cancellationToken = default)
        {
            await s.ReadAsync(Buffer.AsMemory(), cancellationToken);

            return Buffer[0];
        }
    }
}
