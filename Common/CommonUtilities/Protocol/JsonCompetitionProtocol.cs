#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Common.Protocol.CompetitionMessages;
using System.Text.Json.Serialization;

namespace Common.Protocol
{
    public class JsonCompetitionProtocol : ICompetitionProtocol<IJsonProtocolMessage>
    {
        public ICompetitionProtocol<string> UnderlyingProtocol;
        public event EventHandler? OnDisconnect;
        private readonly List<InternalMessageHandler> MessageHandlers = new();
        private readonly ILogger? _logger;

        public JsonCompetitionProtocol(string host, int port, ILogger? logger = null)
            : this(new TcpClient(host, port), logger)
        {
        }

        public JsonCompetitionProtocol(TcpClient socket, ILogger? logger = null)
            : this(new StreamCompetitionProtocol(socket.GetStream()), logger)
        {
        }

        public JsonCompetitionProtocol(ICompetitionProtocol<string> underlyingProtocol, ILogger? logger = null)
        {
            UnderlyingProtocol = underlyingProtocol;

            UnderlyingProtocol.AddMessageHandler<string>((source, message) =>
            {
                try
                {
                    foreach (var handler in MessageHandlers)
                    {
                        handler.TryAccept(message);
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Could not handle message: {message}");
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
                    TransferredTimestamp = heartbeat.Timestamp,
                    ResponseGuid = heartbeat.ResponseGuid
                };

                await SendMessage(acknowledge);
            });
        }

        public void StartConnection()
        {
            UnderlyingProtocol.StartConnection();
        }

        public async Task GetSessionCompletionTask(CancellationToken cancellationToken = default)
        {
            var task = UnderlyingProtocol.GetSessionCompletionTask(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAny(
                    Task.Delay(500, cancellationToken).ContinueWith(res => 0),
                    task.ContinueWith(res => 0)
                );

                if (task.IsCompleted)
                {
                    return;
                }

                await SendHeartbeat(cancellationToken);
            }
        }

        public int AddMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler)
            where TMessageData : IJsonProtocolMessage
        {
            MessageHandlers.Add(new InternalMessageHandler(
                _logger,
                typeof(TMessageData),
                (source, param) =>
                {
                    messageHandler(source, (TMessageData)param);
                },
                (obj) => true
            ));

            return MessageHandlers.Count - 1;
        }

        public void RemoveMessageHandler(int messageHandlerId)
        {
            MessageHandlers.RemoveAt(messageHandlerId);
        }

        public Action AddOnceMessageHandler<TMessageData>(EventHandler<TMessageData> messageHandler, Func<TMessageData, bool>? acceptCondition)
            where TMessageData : IJsonProtocolMessage
        {
            InternalMessageHandler? handlerPointer = null;
            var handler = new InternalMessageHandler(
                _logger,
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

            MessageHandlers.Add(handler);

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

            if (timeout > 0)
            {
                combinedToken.CancelAfter(timeout);
            }

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

        public async Task<TMessageData> SendMessage<TMessageData>(TMessageData messageData, CancellationToken cancellationToken = default)
        {
            var converterOptions = new JsonSerializerOptions
            {
                IncludeFields = true
            };
            converterOptions.Converters.Add(new CompetitionMessageConverter(typeof(TMessageData)));

            var messageJson = JsonSerializer.Serialize(messageData, converterOptions);

            await UnderlyingProtocol.SendMessage(messageJson, cancellationToken);

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
            var msg = await SendMessage(new Heartbeat { }, cancellationToken);

            return await GetResponseAsync<HeartbeatAcknowledge, Heartbeat>(msg, cancellationToken: cancellationToken);
        }

        private class InternalMessageHandler
        {
            public Type Type { get; set; }
            public EventHandler<object> MessageHandler { get; set; }
            public Func<object, bool> AcceptCondition { get; set; }
            private ILogger? _logger { get; set; }

            public InternalMessageHandler(ILogger? logger, Type type, EventHandler<object> eventHandler, Func<object, bool> acceptCondition)
            {
                Type = type;
                MessageHandler = eventHandler;
                AcceptCondition = acceptCondition;
                _logger = logger;
            }

            public bool TryAccept(string message)
            {
                try
                {
                    var converterOptions = new JsonSerializerOptions();
                    converterOptions.Converters.Add(new CompetitionMessageConverter(Type));
                    var result = (CompetitionMessage?)JsonSerializer.Deserialize(message, Type, converterOptions);

                    if (result is CompetitionMessage msg)
                    {
                        try
                        {
                            if (AcceptCondition(msg))
                            {
                                MessageHandler(null, msg);

                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            _logger?.LogError(e, $"Could not handle message {message}");
                            return false;
                        }
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

    internal class CompetitionMessageConverter : JsonConverter<CompetitionMessage>
    {
        private enum TypeDiscriminator
        {
            CommandAcknowledge = 0,
            CompetitionError = 1,
            Heartbeat = 2,
            HeartbeatAcknowledge = 3,

            Login = 4,
            RegisterVM = 5,
            RequestsTeamList = 6,
            RequestsSystemsList = 7,
            TeamsList = 8,
            SystemsList = 9,
            SetTasks = 10,
            SetPenalties = 11
        }

        private readonly string FieldName = "TypeDiscriminator";

        public Type Type { get; }

        public CompetitionMessageConverter(Type type)
        {
            Type = type;
        }

        public override bool CanConvert(Type type)
        {
            return typeof(CompetitionMessage).IsAssignableFrom(type);
        }

        public override CompetitionMessage Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read()
                    || reader.TokenType != JsonTokenType.PropertyName
                    || reader.GetString() != FieldName)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();

            if (!reader.Read() || reader.GetString() != "TypeValue")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            CompetitionMessage? baseClass = typeDiscriminator switch
            {
                TypeDiscriminator.CommandAcknowledge => JsonSerializer.Deserialize<CommandAcknowledge>(ref reader),
                TypeDiscriminator.CompetitionError => JsonSerializer.Deserialize<CompetitionError>(ref reader),
                TypeDiscriminator.Heartbeat => JsonSerializer.Deserialize<Heartbeat>(ref reader),
                TypeDiscriminator.HeartbeatAcknowledge => JsonSerializer.Deserialize<HeartbeatAcknowledge>(ref reader),
                TypeDiscriminator.Login => JsonSerializer.Deserialize<Login>(ref reader),
                TypeDiscriminator.RegisterVM => JsonSerializer.Deserialize<RegisterVM>(ref reader),
                TypeDiscriminator.RequestsTeamList => JsonSerializer.Deserialize<Requests.GetTeams>(ref reader),
                TypeDiscriminator.RequestsSystemsList => JsonSerializer.Deserialize<Requests.GetCompetitionSystems>(ref reader),
                TypeDiscriminator.TeamsList => JsonSerializer.Deserialize<TeamsList>(ref reader),
                TypeDiscriminator.SystemsList => JsonSerializer.Deserialize<CompetitionSystemsList>(ref reader),
                TypeDiscriminator.SetTasks => JsonSerializer.Deserialize<SetCompletedTasks>(ref reader),
                TypeDiscriminator.SetPenalties => JsonSerializer.Deserialize<SetAppliedPenalties>(ref reader),
                _ => throw new NotSupportedException(),
            };

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            if (baseClass is null)
            {
                throw new JsonException();
            }

            if (Type.IsAssignableFrom(baseClass.GetType()))
            {
                return baseClass;
            }
            else
            {
                throw new JsonException();
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            CompetitionMessage value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            void WriteObj(TypeDiscriminator discriminator, CompetitionMessage message)
            {
                writer.WriteNumber(FieldName, (int)discriminator);
                writer.WritePropertyName("TypeValue");
                JsonSerializer.Serialize(writer, message, Type);
            }

            WriteObj(
                value switch
                {
                    CommandAcknowledge _ => TypeDiscriminator.CommandAcknowledge,
                    CompetitionError _ => TypeDiscriminator.CompetitionError,
                    Heartbeat _ => TypeDiscriminator.Heartbeat,
                    HeartbeatAcknowledge _ => TypeDiscriminator.HeartbeatAcknowledge,
                    Login _ => TypeDiscriminator.Login,
                    RegisterVM _ => TypeDiscriminator.RegisterVM,
                    Requests.GetTeams _ => TypeDiscriminator.RequestsTeamList,
                    Requests.GetCompetitionSystems _ => TypeDiscriminator.RequestsSystemsList,
                    TeamsList _ => TypeDiscriminator.TeamsList,
                    CompetitionSystemsList _ => TypeDiscriminator.SystemsList,
                    SetCompletedTasks _ => TypeDiscriminator.SetTasks,
                    SetAppliedPenalties _ => TypeDiscriminator.SetPenalties,
                    _ => throw new NotSupportedException()
                },
                value
            );

            writer.WriteEndObject();
        }
    }
}
