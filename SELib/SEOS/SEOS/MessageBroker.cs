using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public interface IMessageBroker
    {
        long Id { get; }
        TimeSpan AwaitAnswerTime { get; }
        void Set(object cmd);
        void Set(object cmd, string channel);
        void Set(object cmd, long id);
        
        void Set<T>(object cmd, T data);
        void Set<T>(object cmd, long id, T dat);
        
        void Get<T>(object cmd, Action<IEnumerable<KeyValuePair<long, T>>> ans);
        void Get<T>(object cmd, string channel, Action<IEnumerable<KeyValuePair<long, T>>> ans);
        void Get<T>(object cmd, long id, Action<T> ans);

        void Post<T>(object cmd, Func<T> getter);
        void Post<T>(object cmd, Func<long, T> getter);
        void Post<T>(object cmd, long id, Func<T> getter);
        void Post<T>(object cmd, string channel, Func<T> getter);
        void Post<T>(object cmd, string channel, Func<long, T> getter);
        
        void Post<T>(object cmd, Action<T> setter);
        void Post<T>(object cmd, long id, Action<T> setter);
        void Post<T>(object cmd, string channel, Action<T> setter);
        void Post<T>(object cmd, string channel, Action<long, T> setter);
        
        void Post(object cmd, Action post);
        void Post(object cmd, long id, Action post);
        void Post(object cmd, string channel, Action post);
        void Post(object cmd, string channel, Action<long> post);
    }
     class MessageBroker : IMessageBroker
    {
        const string SHARED_RADIO_CHANNEL = "SEOS radio channel";
        public long Id { get; }
        public TimeSpan AwaitAnswerTime { get; } = TimeSpan.FromSeconds(2);
        struct NullType
        { }
        struct SenderInfo
        {
            const char DELIMETER = '#';
            public string Cmd;
            public string DataType;

            public bool IsEquals(SenderInfo b)
            {
                return b.Cmd == Cmd && b.DataType == DataType;
            }

            public string CreateTag() => string.Join(DELIMETER.ToString(), Cmd, DataType);
            public override string ToString()
            {
                return $"cmd: '{Cmd}'- data type: '{DataType}'";
            }

            public static SenderInfo FromIGCMessage(MyIGCMessage msg)
            {
                if (msg.Tag.Contains(DELIMETER))
                {
                    return Parse(msg.Tag);
                }
                
                var data = msg.As<string>();
                if (data.Contains(DELIMETER))
                    return Parse(data);
                
                return new SenderInfo
                {
                    Cmd = data,
                    DataType = nameof(NullType)
                };
            }

            static SenderInfo Parse(string line)
            {
                var tagArr = line.Split(DELIMETER);
                return new SenderInfo
                {
                    Cmd = tagArr[0],
                    DataType = tagArr[1]
                };
            }
        }

        interface IPoster
        {
            bool TryPost(IMyIntergridCommunicationSystem igc, MyIGCMessage msg, string channel = "", ILogger logger = null);
            void TrySetInternal<T>(string cmd, T data, ILogger logger);
            T TryGetInternal<T>(string cmd, ILogger logger);

        }
        class Poster<T> : IPoster
        {
            SenderInfo _info;
            string _channel;
            long _id;
            ISerializer _serialize;

            Action<long, T> _setter;
            Func<long, T> _getter;
            Poster() { }

            public static IPoster Create(object cmd, Dictionary<string, object> serializers, Func<T> getter, string channel = "", long from = -1)
            {
                return Create(cmd, serializers, (l) => { return getter();}, channel, from);
            }
            public static IPoster Create(object cmd, IReadOnlyDictionary<string, object> serializers, Func<long, T> getter, string channel = "", long from = -1)
            {
                var type = typeof(T).Name;
                var s = serializers.ContainsKey(type)
                    ? (ISerializer)serializers[type]
                    : null;

                return new Poster<T>()
                {
                    _info = new SenderInfo
                    {
                        DataType = type,
                        Cmd = cmd.ToString(),

                    },
                    _id = from,
                    _channel = channel,
                    _serialize = s,
                    _getter = getter,
                };
            }
            public static IPoster Create(object cmd, Dictionary<string, object> serializers, Action setter, string channel = "", long from = -1)
            {
                return Create(cmd, serializers, (l, t) => setter(), channel, from);
            }
            public static IPoster Create(object cmd, Dictionary<string, object> serializers, Action<long> setter, string channel = "", long from = -1)
            {
                return Create(cmd, serializers, (l, t) => setter(l), channel, from);
            }
            public static IPoster Create(object cmd, Dictionary<string, object> serializers, Action<T> setter, string channel = "", long from = -1)
            {
                return Create(cmd, serializers, (l, t) => setter(t), channel, from);
            }
            public static IPoster Create(object cmd, Dictionary<string, object> serializers, Action<long, T> setter, string channel = "", long from = -1)
            {
                var type = typeof(T).Name;
                var s = serializers.ContainsKey(type)
                    ? (ISerializer)serializers[type]
                    : null;

                return new Poster<T>
                {
                    _info = new SenderInfo
                    {
                        DataType = type,
                        Cmd = cmd.ToString(),
                    },
                    _id = from,
                    _channel = channel,
                    _serialize = s,
                    _setter = setter
                };
            }

            bool IsMyMessage(MyIGCMessage msg, string channel = "")
            {
                if (_id == msg.Source || _id == -1)
                    return true;

                return channel == _channel || _channel == "";
            }

            public bool TryPost(IMyIntergridCommunicationSystem igc, MyIGCMessage msg, string channel = "", ILogger logger = null)
            {
                var requester = msg.Source;

                var info = SenderInfo.FromIGCMessage(msg);
                //logger?.Log(logLevel, $"Info: {_info}-{info}");
                if (!_info.IsEquals(info)) return false;
                //logger?.Log(logLevel, $"Info is OK");
                
                if (!IsMyMessage(msg, channel)) return false;

                if (_getter != null)
                {
                    var tag = _info.CreateTag();
                    if (_serialize == null)
                    {
                        var data = _getter(requester);
                        logger.Log(NoteLevel.Info, $"Send br msg. Data= {data}");
                        igc.SendUnicastMessage(requester, tag, data);
                    }
                    else
                    {
                        var data = ((ISerializer) _getter(requester)).Serialize();
                        logger.Log(NoteLevel.Info, $"Send un. msg. Data=");
                        foreach (var item in data)
                        {
                            logger.Log(NoteLevel.Info, $"{item}");
                        }
                        igc.SendUnicastMessage(requester, tag, data);
                    }

                }
                else if (_setter != null)
                {
                    if (_serialize == null)
                    {
                        logger?.Log(NoteLevel.Waring, $"serializer is NULL");
                        _setter(requester, msg.As<T>());
                    }
                    else
                    {
                        try
                        {
                            var data1 = msg.As<ImmutableArray<string>>();
                            _setter(requester, (T) _serialize.Deserialize(ref data1));

                        }
                        catch (Exception e)
                        {
                            try
                            {
                                _setter(requester, default(T));
                            }
                            catch (Exception exception)
                            {
                                logger?.Log(exception);
                            }
                        }
                    }
                }

                return true;
            }

            public void TrySetInternal<TT>(string cmd, TT data, ILogger logger = null)
            {
                if (typeof(T) != typeof(TT)) return;
                
                // предпологается, что это комманда из терминала и с параметрами
                if (!cmd.Contains(_info.Cmd)) return;

                if (data == null || data.Equals(default(T)))
                    _setter?.Invoke(-1, default(T));
                else
                {
                    var s = (object) data;
                    _setter?.Invoke(-1, (T)s); 
                }
            }
            public TT TryGetInternal<TT>(string cmd, ILogger logger)
            {
                //logger?.Log(LogMessageType.Info, $"{typeof(T).Name}-{typeof(TT).Name}");
                if (typeof(TT) != typeof(T) || _getter == null) return default(TT);
                
                if (_info.Cmd != cmd) return default(TT);
                /*
                 * if (_info.DataType != typeof(T).Name) return default(TT);
                */

                var s = _getter(-1) as object;
                if (s == null) return default(TT);
                
                return (TT) s;
            }
        }
        class Getter
        {
            public bool IsConcreteGetter => _id != -1;
            public bool HasAnswer { get; private set; }
            SenderInfo _info;
            string _channel = "";
            long _id = -1;
            
            public readonly DateTime CreateDate = DateTime.Now;

            List<KeyValuePair<long, object>> _result;
            Action<IEnumerable<KeyValuePair<long, object>>> _ans;

            Getter() { }

            public static Getter Create<T>(object cmd, Dictionary<string, object> serializers, string channel,
                Action<IEnumerable<KeyValuePair<long, T>>> ans, IEnumerable<KeyValuePair<long, T>> initial)
            {
                var type = typeof(T).Name;
                var serializer = serializers.ContainsKey(type)
                    ? (ISerializer) serializers[type]
                    : null;

                var retVal = new Getter
                {
                    _info = new SenderInfo {Cmd = cmd.ToString(), DataType = type,},
                    _channel = channel,
                    _ans = pairs =>
                    {
                        var data = new List<KeyValuePair<long, T>>();
                        foreach (var pair in pairs)
                        {
                            if (serializer == null)
                                data.Add(new KeyValuePair<long, T>(pair.Key, (T) pair.Value));
                            else
                            {
                                var imm = (ImmutableArray<string>) pair.Value;
                                data.Add(new KeyValuePair<long, T>(pair.Key, (T) serializer.Deserialize(ref imm)));
                            }
                        }

                        ans(data);
                    },
                    _result = ToObjData(initial),
                };


                return retVal;
            }

            public static Getter Create<T>(object cmd, Dictionary<string, object> serializers, long id,
                Action<T> ans, IEnumerable<KeyValuePair<long, T>> initial, ILogger logger)
            {
                var type = typeof(T).Name;
                var serializer = serializers.ContainsKey(type)
                    ? (ISerializer) serializers[type]
                    : null;

                var retVal = new Getter
                {
                    _info = new SenderInfo {Cmd = cmd.ToString(), DataType = type},
                    _id = id,
                    _ans = pairs =>
                    {
                        if (pairs == null || pairs.Count() > 1)
                            throw new Exception("Internal error");

                        if (serializer == null)
                            ans((T) pairs.First().Value);
                        else
                        {
                            var imm = (ImmutableArray<string>) pairs.First().Value;
                            logger.Log(NoteLevel.Info, $"Dirty msg data=");
                            foreach (var item in imm)
                            {
                                logger.Log(NoteLevel.Info, $"{item}");
                            }
                            ans((T) serializer.Deserialize(ref imm));
                        }
                    },
                    _result = ToObjData(initial)
                };
                
                return retVal;
            }

            static List<KeyValuePair<long, object>> ToObjData<T>(IEnumerable<KeyValuePair<long, T>> initial)
            {
                var objData = new List<KeyValuePair<long, object>>();
                foreach (var pair in initial)
                {
                    objData.Add(new KeyValuePair<long, object>(pair.Key, pair.Value));
                }

                return objData;
            }

            public bool TryCollect(MyIGCMessage msg, string channel = "")
            {
                var requester = msg.Source;

                {
                    if (_channel == "" && _id != requester) return false;
                    if (channel != "" && _channel != channel) return false;

                    var info = SenderInfo.FromIGCMessage(msg);

                    if (!_info.IsEquals(info)) return false;
                }
                

                
                _result.Add(new KeyValuePair<long, object>(requester, msg.Data));
                HasAnswer = true;

                return true;
            }

            public void Send(IMyIntergridCommunicationSystem igc, SenderInfo info, string channel)
            {
                igc.SendBroadcastMessage(channel, info.CreateTag());
            }
            public void Send(IMyIntergridCommunicationSystem igc, SenderInfo info, long id)
            {
                igc.SendUnicastMessage(id, info.CreateTag(), "");
            }

            public void Ans()
            {
                _ans(_result);
            }
        }


        public Dictionary<string, object> Serializers = new Dictionary<string, object>();
        
        IMyIntergridCommunicationSystem _igc;
        ILogger _logger;
        
        List<IPoster> _posters = new List<IPoster>();
        List<Getter> _getters = new List<Getter>();

        public MessageBroker(long me, ILogger logger)
        {
            _igc = null;
            Id = me;
            _logger = logger;
        }

        public MessageBroker(SEOS os, IEnumerable<string> channels, IMyTerminalBlock antenna)
        {
            _igc = os.Program.IGC;
            Id = _igc.Me;
            _logger = os.Logger;

            var allChannels = new List<string> {SHARED_RADIO_CHANNEL};
            foreach (var channel in channels)
            {
                _igc.RegisterBroadcastListener(channel);
                allChannels.Add(channel);
            }
        }

        public void Tick(string argument)
        {
            if (!string.IsNullOrEmpty(argument))
                Set(argument);
            
            var gettersToRemove = new List<Getter>();
            foreach (var getter in _getters)
            {
                if (DateTime.Now >= (getter.CreateDate + AwaitAnswerTime) ||
                    getter.IsConcreteGetter && getter.HasAnswer)
                {
                    gettersToRemove.Add(getter);
                    getter.Ans();
                }
            }
            foreach (var getter in gettersToRemove)
            {
                _getters.Remove(getter);
            }

            if (_igc == null) return;

            var bListeners = new List<IMyBroadcastListener>();
            _igc.GetBroadcastListeners(bListeners);

            var messages = new List<KeyValuePair<string, MyIGCMessage>>();

            foreach (var provider in bListeners)
            {
                while (provider.HasPendingMessage)
                    messages.Add(new KeyValuePair<string, MyIGCMessage>(provider.Tag, provider.AcceptMessage()));
            }

            while (_igc.UnicastListener.HasPendingMessage)
                messages.Add(new KeyValuePair<string, MyIGCMessage>("", _igc.UnicastListener.AcceptMessage()));

            if (messages.Any())
                AcceptMsg(messages);
        }

        void AcceptMsg(IEnumerable<KeyValuePair<string, MyIGCMessage>> messages)
        {
            var logLevel = NoteLevel.Info;
            //_logger.Log(logLevel, "===================");

            foreach (var message in messages)
            {
                // _logger.Log(logLevel, $"from channel='{message.Key}'");
                // _logger.Log(logLevel, $"     from id='{message.Value.Source};");
                // _logger.Log(logLevel, $"         tag='{message.Value.Tag}'");
                // _logger.Log(logLevel, $" data={message.Value.Data}");
                
                //_logger.Log(logLevel, $"Try append msg for getters");
                var exit = false;
                foreach (var getter in _getters)
                {
                    exit = (getter.TryCollect(message.Value, message.Key));
                    if (exit) break;
                }
                
                if (exit) continue;
                
                //_logger.Log(logLevel, $"Try append msg for posters");
                foreach (var poster in _posters)
                {
                    if (poster.TryPost(_igc, message.Value, message.Key, _logger))
                        _logger.Log(logLevel, $"MSG append");
                }
            }

            //_logger.Log(logLevel, "===================");
        }

        public void Set(object cmd)
        {
            foreach (var poster in _posters)
            {
                var cmdStr = cmd.ToString();
                poster.TrySetInternal(cmdStr, default(NullType), _logger);
                poster.TrySetInternal(cmdStr, cmdStr, _logger);
            }
        }

        public void Set(object cmd, string channel)
        {
            //_logger.Log(LogMessageType.Info, $"Recive msg '{channel}'-'{cmd}'");
            _igc.SendBroadcastMessage(channel, cmd.ToString());
        }

        public void Set(object cmd, long id)
        {
            //_logger.Log(LogMessageType.Info, $"Recive msg '{id}'-'{cmd}'");
            _igc.SendUnicastMessage(id, cmd.ToString(), "");
        }

        public void Set<T>(object cmd, T data)
        {
            //_logger.Log(LogMessageType.Info, $"Recive msg for internal'{cmd}'-'{data}'");
            foreach (var poster in _posters)
            {
                poster.TrySetInternal(cmd.ToString(), data, _logger);
            }
        }

        public void Set<T>(object cmd, long id, T data)
        {
            //_logger.Log(LogMessageType.Info, $"Recive msg for '{id}-{cmd}'-'{data}'");

            var serializer = data as ISerializer;
            if (serializer != null)
                _igc.SendUnicastMessage(id, cmd.ToString(), serializer.Serialize());
            else
                _igc.SendUnicastMessage(id, cmd.ToString(), data);
        }

        public void Get<T>(object cmd, Action<IEnumerable<KeyValuePair<long, T>>> ans)
        {
            //_logger.Log(LogMessageType.Info, $"Get '{cmd}'");
            var retVal = InternalGet<T>(cmd);
            ans(retVal);
        }

        public void Get<T>(object cmd, string channel, Action<IEnumerable<KeyValuePair<long, T>>> ans)
        {
            //_logger.Log(LogMessageType.Info, $"Get '{channel}-{cmd}'");
            var retVal = InternalGet<T>(cmd);

            var getter = Getter.Create(cmd, Serializers, channel, ans, retVal);
            _getters.Add(getter);
            getter.Send(_igc, new SenderInfo{Cmd = cmd.ToString(), DataType = typeof(T).Name}, channel);
        }

        public void Get<T>(object cmd, long id, Action<T> ans)
        {
            //_logger.Log(LogMessageType.Info, $"Get '{id}-{cmd}'");
            var retVal = InternalGet<T>(cmd).ToList();

            var getter = Getter.Create(cmd, Serializers, id, ans, retVal, _logger);
            _getters.Add(getter);
            if (!retVal.Any())
                getter.Send(_igc, new SenderInfo{Cmd = cmd.ToString(), DataType = typeof(T).Name}, id);
        }

        IEnumerable<KeyValuePair<long, T>> InternalGet<T>(object cmd)
        {
            var retVal = new List<KeyValuePair<long, T>>();
            foreach (var poster in _posters)
            {
                var data = poster.TryGetInternal<T>(cmd.ToString(), _logger);

                if (data != null && !data.Equals(default(T)))
                {
                    retVal.Add(new KeyValuePair<long, T>(Id, data));
                }
            }

            return retVal;
        }


        public void Post<T>(object cmd, Func<T> getter)
        {

            _posters.Add(Poster<T>.Create(cmd, Serializers, getter));
        }

        public void Post<T>(object cmd, Func<long, T> getter)
        {
            // TODO не осмысливал работы этого метода
            _posters.Add(Poster<T>.Create(cmd, Serializers, getter));
        }

        public void Post<T>(object cmd, long id, Func<T> getter)
        {

            _posters.Add(Poster<T>.Create(cmd, Serializers, getter, from:id));
        }

        public void Post<T>(object cmd, string channel, Func<T> getter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, getter, channel));
        }

        public void Post<T>(object cmd, string channel, Func<long, T> getter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, getter, channel));
        }

        public void Post<T>(object cmd, Action<T> setter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, setter));
        }

        public void Post<T>(object cmd, long id, Action<T> setter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, setter, from:id));
        }

        public void Post<T>(object cmd, string channel, Action<T> setter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, setter, channel));
        }

        public void Post<T>(object cmd, string channel, Action<long, T> setter)
        {
            _posters.Add(Poster<T>.Create(cmd, Serializers, setter, channel));
        }
        public void Post(object cmd, Action post)
        {
            _posters.Add(Poster<NullType>.Create(cmd, Serializers, post));
        }

        public void Post(object cmd, long id, Action post)
        {
            _posters.Add(Poster<NullType>.Create(cmd, Serializers, post, from:id));
        }

        public void Post(object cmd, string channel, Action post)
        {
            _posters.Add(Poster<NullType>.Create(cmd, Serializers, post, channel));
        }

        public void Post(object cmd, string channel, Action<long> post)
        {
            _posters.Add(Poster<NullType>.Create(cmd, Serializers, post, channel));
        }
    }
}