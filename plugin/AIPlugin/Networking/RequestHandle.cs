using AIPlugin.Utilities;
using System;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public interface IPayload { }
    public interface IResponse { }

    public class RefreshableRequest<TPayload, TResponse> : IDisposable
    where TResponse : class, IResponse
    where TPayload : class, IPayload
    {
        public TResponse Result { get; private set; } = null;
        public string Log { get; private set; } = "";

        public int ResultVersion = 0;

        private RequestHandle<TPayload, TResponse> _requestTask = null;
        public Task PendingTask { get; private set; }

        private AiGateway _gateway;

        private Func<RequestHandle<TPayload, TResponse>> _requestFactory;

        public bool RetryOnFailure = false;

        public event Action<TResponse> OnSuccess;
        public event Action<string> OnError;
        public RefreshableRequest(AiGateway gateway, Func<RequestHandle<TPayload, TResponse>> requestFactory)
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
            await _requestTask.AwaitResponseTask;

            if (!_requestTask.FinishedWithSuccess() && RetryOnFailure)
            {
                PendingTask = null;
                Send();
                return;
            }
            else if (!_requestTask.FinishedWithSuccess() && !RetryOnFailure)
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    OnError?.Invoke(_requestTask.Log);
                });
            }

            try
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Result = _requestTask.Result;
                    Log = _requestTask.Log;

                    ResultVersion++;

                    OnSuccess?.Invoke(Result);
                });

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

    /// <summary>
    /// Starts the asynchronous task that sends request to the server and awaits the response.
    /// </summary>
    public class RequestHandle<TPayload, TResponse> : IDisposable
    where TResponse : class, IResponse
    where TPayload : class, IPayload
    {
        public readonly Task AwaitResponseTask;
        private bool _success = false;
        public TResponse Result { get; private set; } = null;
        public string Log { get; private set; } = "";

        public event Action<TResponse> OnSuccess;
        public event Action<string> OnError;

        public RequestHandle(AiGateway gateway, string type, TPayload payload)
        {
            AwaitResponseTask = AwaitResponseAsync(gateway, type, payload);
        }
        private async Task AwaitResponseAsync(AiGateway gateway, string type, TPayload payload)
        {
            try
            {
                var (result, log) = await gateway.SendRequestAsync<TResponse, TPayload>(type, payload);

                Result = result;
                Log = log;
                _success = true;

                MainThreadDispatcher.Enqueue(() =>
                {
                    OnSuccess?.Invoke(Result);
                });
            }
            catch (Exception ex)
            {
                Log = ex.ToString();
                MainThreadDispatcher.Enqueue(() =>
                {
                    OnError?.Invoke(Log);
                });
            }
        }
        public bool Finished() => AwaitResponseTask.IsCompleted;
        public bool FinishedWithSuccess() => Finished() && _success;
        public void Dispose() // TODO add cancelation token support
        {
        }
    }

}
