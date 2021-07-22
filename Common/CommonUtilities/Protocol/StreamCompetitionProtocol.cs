using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Protocol
{
    public class StreamCompetitionProtocol : ICompetitionProtocol<string>
    {
        private readonly static Encoding encoding = Encoding.UTF8;

        private readonly byte[] SyncBytes = encoding.GetBytes("COMPSYNC");

        public event EventHandler OnDisconnect;

        public Stream Stream { get; init; }
        private List<EventHandler<string>> EventHandlers { get; } = new();
        private Task Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; } = new();

        private List<string> MessageQueue { get; set; } = new();

        /**
         * Consumes the Stream provided
         */
        public StreamCompetitionProtocol(Stream stream)
        {
            Stream = stream;

            Task = ListenOnStream();
        }

        public void StartConnection()
        {
            var storedMessages = MessageQueue;
            MessageQueue = null;

            foreach (var msg in storedMessages)
            {
                foreach (var handler in EventHandlers)
                {
                    handler(null, msg);
                }
            }
        }

        int ICompetitionProtocol<string>.AddMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler)
        {
            EventHandlers.Add((source, msg) =>
            {
                messageHandler(source, msg as TMessageData);
            });

            return EventHandlers.Count - 1;
        }

        public void RemoveMessageHandler(int id)
        {
            EventHandlers.RemoveAt(id);
        }

        async Task<TMessageData> ICompetitionProtocol<string>.SendMessage<TMessageData>(TMessageData messageData, CancellationToken cancellationToken)
        {
            var data = messageData as string;
            var bytes = encoding.GetBytes(data);
            var lengthBytes = BitConverter.GetBytes(bytes.Length);

            var finalBytes = new byte[SyncBytes.Length + lengthBytes.Length + bytes.Length];

            Buffer.BlockCopy(SyncBytes, 0, finalBytes, 0, SyncBytes.Length);
            Buffer.BlockCopy(lengthBytes, 0, finalBytes, SyncBytes.Length, lengthBytes.Length);
            Buffer.BlockCopy(bytes, 0, finalBytes, SyncBytes.Length + lengthBytes.Length, bytes.Length);

            await Stream.WriteAsync(finalBytes, cancellationToken);
            await Stream.FlushAsync(cancellationToken);

            return messageData;
        }

        public Task GetSessionCompletionTask(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => CancellationTokenSource.Cancel());
            return Task;
        }

        public async ValueTask DisposeAsync()
        {
            CancellationTokenSource.Cancel();

            await Task;

            await Stream.DisposeAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            DisposeAsync().AsTask().Wait();
        }

        private async Task ListenOnStream()
        {
            var buffer = new byte[SyncBytes.Length];

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    for (var i = 0; i < SyncBytes.Length; i++)
                    {
                        buffer[i] = 0;
                    }

                    do
                    {
                        for (var i = 1; i < SyncBytes.Length; i++)
                        {
                            buffer[i - 1] = buffer[i];
                        }

                        buffer[SyncBytes.Length - 1] = await Stream.ReadByteAsync(CancellationTokenSource.Token);
                    } while (!SyncBytes.SequenceEqual(buffer) && !CancellationTokenSource.IsCancellationRequested);

                    var bufferLength = new byte[4];
                    await Stream.ReadAsync(bufferLength.AsMemory(), CancellationTokenSource.Token);
                    var packetLength = BitConverter.ToInt32(bufferLength.AsSpan());

                    var packet = new byte[packetLength];
                    await Stream.ReadAsync(packet.AsMemory(), CancellationTokenSource.Token);

                    var value = encoding.GetString(packet);

                    if (MessageQueue is List<string> queue)
                    {
                        queue.Add(value);
                    }
                    else
                    {
                        foreach (var handler in EventHandlers)
                        {
                            handler(null, value);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is AggregateException ex)
                    {
                        ex.Handle(_ => true);
                        OnDisconnect?.Invoke(null, null);
                    }
                    else if (e is InvalidOperationException or TaskCanceledException or OperationCanceledException or IOException or SocketException)
                    {
                        OnDisconnect?.Invoke(null, null);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
