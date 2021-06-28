#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Common.Protocol.CompetitionMessages;

namespace Common.Protocol
{
    public class JsonCompetitionProtocol : ICompetitionProtocol<IJsonProtocolMessage>
    {
        public ICompetitionProtocol<string> UnderlyingProtocol;
        public event EventHandler? OnDisconnect;
        private readonly LinkedList<InternalMessageHandler> MessageHandlers = new();
        private readonly ILogger? _logger;
        private readonly List<string> CompetitionMessages = new();

        public JsonCompetitionProtocol(TcpClient socket, ILogger? logger = null)
            : this(new StreamCompetitionProtocol(socket.GetStream()), logger)
        {
        }

        public JsonCompetitionProtocol(ICompetitionProtocol<string> underlyingProtocol, ILogger? logger = null)
        {
            UnderlyingProtocol = underlyingProtocol;

            UnderlyingProtocol.AddMessageHandler<string>((source, message) =>
            {
                var accepted = false;

                foreach (var handler in MessageHandlers)
                {
                    accepted = handler.TryAccept(message) || accepted;
                }

                if (!accepted)
                {
                    CompetitionMessages.Add(message);
                }
            });

            UnderlyingProtocol.OnDisconnect += (source, ev) =>
            {
                OnDisconnect?.Invoke(source, ev);
            };

            _logger = logger;

            AddMessageHandler<Heartbeat>(async (source, heartbeat) =>
            {
                var acknowledge = new HeartbeatAcknowledge
                {
                    TransferredTimestamp = heartbeat.Timestamp
                };

                await SendMessage(acknowledge);
            });
        }

        public Task GetSessionCompletionTask(CancellationToken cancellationToken = default) => UnderlyingProtocol.GetSessionCompletionTask(cancellationToken);

        public void AddMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler)
            where TMessageData : IJsonProtocolMessage
        {
            MessageHandlers.AddLast(new InternalMessageHandler(
                typeof(TMessageData),
                (source, param) =>
                {
                    messageHandler(source, (TMessageData)param);
                },
                (obj) => true
            ));
        }

        public Action AddOnceMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler, Func<TMessageData, bool>? acceptCondition)
            where TMessageData : IJsonProtocolMessage
        {
            InternalMessageHandler? handlerPointer = null;
            var handler = new InternalMessageHandler(
                typeof(TMessageData),
                (source, param) =>
                {
                    messageHandler(source, (TMessageData)param);

                    if (handlerPointer is InternalMessageHandler hp)
                    {
                        MessageHandlers.Remove(hp);
                    }
                },
                acceptCondition switch
                {
                    null => (obj) => true,
                    Func<TMessageData, bool> func => (obj) => func((TMessageData)obj)
                }
            );
            handlerPointer = handler;

            MessageHandlers.AddLast(handler);

            return () =>
            {
                MessageHandlers.Remove(handler);
            };
        }

        public Task<TMessageData> GetMessageAsync<TMessageData>(int timeout = 3000, Func<TMessageData, bool>? acceptCondition = null, CancellationToken cancellationToken = default)
            where TMessageData : IJsonProtocolMessage
        {
            var tcs = new TaskCompletionSource<TMessageData>();
            var cts = new CancellationTokenSource();

            using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
            combinedToken.Token.Register(() => tcs.TrySetCanceled());

            combinedToken.CancelAfter(timeout);

            AddOnceMessageHandler((source, param) =>
            {
                tcs.SetResult(param);
            },
            acceptCondition);

            return tcs.Task;
        }

        public async Task<TResponseData> GetResponseAsync<TResponseData, TMessageData>(TMessageData messageData, int timeout = 3000, CancellationToken cancellationToken = default)
            where TResponseData : CompetitionMessage
            where TMessageData : CompetitionMessage
        {
            var task = Task.WhenAny(
                GetMessageAsync<TResponseData>(timeout, (msg) => msg.ResponseGuid == messageData.ResponseGuid, cancellationToken)
                    .ContinueWith(res => (CompetitionMessage)res.Result),
                GetMessageAsync<CompetitionError>(timeout, (msg) => msg.ResponseGuid == messageData.ResponseGuid, cancellationToken)
                    .ContinueWith(res => (CompetitionMessage)res.Result)
            );

            var response = await await task;

            return response switch
            {
                CompetitionError err => throw new Exception(err.ErrorMessage),
                TResponseData res => res,

                _ => throw new Exception("Unknown message type")
            };
        }

        public Task GetAcknowledgementAsync<TMessageData>(TMessageData message, int timeout = 3000, CancellationToken cancellationToken = default)
            where TMessageData : CompetitionMessage
        {
            return GetResponseAsync<CommandAcknowledge, TMessageData>(message, timeout, cancellationToken);
        }

        public async Task<TMessageData> SendMessage<TMessageData>(TMessageData messageData)
        {
            var prettyJson = JsonSerializer.Serialize(messageData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger?.LogTrace($"Sending pretty JSON:\n{prettyJson}");

            var messageJson = JsonSerializer.Serialize(messageData);

            await UnderlyingProtocol.SendMessage(messageJson);

            return messageData;
        }

        public ValueTask DisposeAsync()
        {
            return UnderlyingProtocol.DisposeAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            UnderlyingProtocol.Dispose();
        }

        public async Task<HeartbeatAcknowledge> SendHeartbeat(CancellationToken cancellationToken = default)
        {
            var msg = await SendMessage(new Heartbeat { });

            return await GetResponseAsync<HeartbeatAcknowledge, Heartbeat>(msg, cancellationToken: cancellationToken);
        }

        private class InternalMessageHandler
        {
            public Type Type { get; set; }
            public EventHandler<object> MessageHandler { get; set; }
            public Func<object, bool> AcceptCondition { get; set; }

            public InternalMessageHandler(Type type, EventHandler<object> eventHandler, Func<object, bool> acceptCondition)
            {
                Type = type;
                MessageHandler = eventHandler;
                AcceptCondition = acceptCondition;
            }

            public bool TryAccept(string message)
            {
                try
                {
                    var result = JsonSerializer.Deserialize(message, Type);

                    if (result == null)
                    {
                        return false;
                    }

                    if (AcceptCondition(result))
                    {
                        MessageHandler(null, result);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    public interface IJsonProtocolMessage
    {
    }
}
