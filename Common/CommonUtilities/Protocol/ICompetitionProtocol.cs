#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Protocol
{
    public interface ICompetitionProtocol<TBaseMessageType> : IDisposable, IAsyncDisposable
    {
        public event EventHandler? OnDisconnect;

        public Task GetSessionCompletionTask(CancellationToken cancellationToken = default);


        public int AddMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler) where TMessageData : TBaseMessageType;
        public void RemoveMessageHandler(int id);


        public Task<TMessageData> SendMessage<TMessageData>(TMessageData messageData, CancellationToken cancellationToken = default);
    }
}
