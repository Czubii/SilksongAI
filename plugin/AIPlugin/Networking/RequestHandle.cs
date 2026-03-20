using System;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public static class ServerRequest
    {
        public class GetArchitectures : RequestHandle<Responses.Architectures>
        {
            public GetArchitectures(AiGateway gateway) : base(gateway, RequestType.get_architectures) { }
        }
        public class GetArtifacts : RequestHandle<Responses.Artifacts>
        {
            public GetArtifacts(AiGateway gateway) : base(gateway, RequestType.get_artifacts) { }
        }
        public class NewArtifact : RequestHandle<EmptyPayload, Requests.NewArtifact>
        {
            public NewArtifact(AiGateway gateway, Requests.NewArtifact payload) : base(gateway, RequestType.new_model, payload) { } // TODO change request name
        }
        public class TrainArtifact : RequestHandle<EmptyPayload, Requests.TrainArtifcat>
        {
            public TrainArtifact(AiGateway gateway, Requests.TrainArtifcat payload) : base(gateway, RequestType.train_artifact, payload) { } // TODO change request name
        }

        public static class Refreshable
        {
            public static Refreshable<TResponse, TPayload> Watch<TResponse, TPayload>(AiGateway gateway, Func<RequestHandle<TResponse, TPayload>> factory)
                where TResponse : class 
                where TPayload : class
            {
                return new Refreshable<TResponse, TPayload>(gateway, factory);
            }

            public static Refreshable<TResponse> Watch<TResponse>(AiGateway gateway, Func<RequestHandle<TResponse>> factory)
                where TResponse : class
            {
                return new Refreshable<TResponse>(gateway, factory);
            }
        }

        public class Refreshable<TResponse, TPayload> : IDisposable
        where TResponse : class
        where TPayload : class
        {
            public TResponse Result { get; private set; } = null;
            public string Log { get; private set; } = "";

            public int ResultVersion = 0;

            private RequestHandle<TResponse, TPayload> _requestTask = null;
            public Task PendingTask { get; private set; }

            private AiGateway _gateway;

            private Func<RequestHandle<TResponse, TPayload>> _requestFactory;

            public Refreshable(AiGateway gateway, RequestType type, TPayload payload)
            {
                _gateway = gateway;
                _requestFactory = () => new RequestHandle<TResponse, TPayload>(gateway, type, payload);
            }

            public Refreshable(AiGateway gateway, Func<RequestHandle<TResponse, TPayload>> requestFactory)
            {
                _gateway = gateway;
                _requestFactory = requestFactory;
            }
            public void Send()
            {
                if (PendingTask != null) return;

                _requestTask = _requestFactory();
                PendingTask = AwaitResponseAsync();
            }
            private async Task AwaitResponseAsync()
            {
                try
                {
                    await _requestTask.AwaitResponseTask;

                    Result = _requestTask.Result;
                    Log = _requestTask.Log;

                    ResultVersion++;
                }
                finally
                {
                    PendingTask = null;
                }
            }
            public bool AnyResponse() => Result != null;
            public void Dispose()
            {
                _requestTask?.Dispose();
            }
        }

        public class Refreshable<TResponse> : Refreshable<TResponse, EmptyPayload>
        where TResponse : class
        {
            public Refreshable(AiGateway gateway, RequestType type): base(gateway, type, new EmptyPayload()) { }
            public Refreshable(AiGateway gateway, Func<RequestHandle<TResponse>> requestFactory) : base(gateway, requestFactory) { }
        }

        /// <summary>
        /// Starts the asynchronous task that sends request to the server and awaits the response.
        /// </summary>
        public class RequestHandle<TResponse, TPayload> : IDisposable
        where TResponse : class
        where TPayload : class
        {
            public readonly Task AwaitResponseTask;
            private bool _success = false;
            public TResponse Result { get; private set; } = null;
            public string Log { get; private set; } = "";
            public RequestHandle(AiGateway gateway, RequestType type, TPayload payload)
            {
                AwaitResponseTask = AwaitResponseAsync(gateway, type, payload);
            }
            private async Task AwaitResponseAsync(AiGateway gateway, RequestType type, TPayload payload)
            {
                try
                {
                    var (result, log) = await gateway.SendRequestAsync<TResponse, TPayload>
                        (type.ToString(), payload);

                    Result = result;
                    Log = log;
                    _success = true;
                }
                catch (Exception ex)
                {
                    Log = ex.ToString();
                }
            }
            public bool Finished() => AwaitResponseTask.IsCompleted;
            public bool FinishedWithSuccess() => Finished() && _success;
            public void Dispose() // TODO add cancelation token support
            {
            }
        }
        public class RequestHandle<TResponse> : RequestHandle<TResponse, EmptyPayload>
        where TResponse : class
        {
            public RequestHandle(AiGateway gateway, RequestType type)
                : base(gateway, type, new EmptyPayload()) { }
        }
    }
}
