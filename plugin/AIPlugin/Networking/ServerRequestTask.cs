using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIPlugin.Networking.AiGateway;

namespace AIPlugin.Networking
{
    /// <summary>
    /// Starts the asynchronous task that sends request to the server and awaits the response.
    /// </summary>
    public class ServerRequestTask<TResponse, TPayload> : IDisposable
    where TResponse : class
    where TPayload : class
    {
        private Task _task;
        private bool _success = false;
        public TResponse Result { get; private set; } = null;
        public string Log { get; private set; } = "";
        public ServerRequestTask(AiGateway gateway, RequestType type, TPayload payload)
        {
            _task = Task.Run(() => AwaitResponseAsync(gateway, type, payload));
        }
        private async Task AwaitResponseAsync(AiGateway gateway, RequestType type, TPayload payload)
        {
            try
            {
                (Result, Log) = await gateway.SendRequestAsync<TResponse, TPayload>
                    (type.ToString(), payload);
                _success = true;
            }
            catch (Exception ex)
            {
                Result = null;
                Log = ex.ToString();
            }
            finally
            {
                _task = null;
            }
        }
        public bool Finished() => _task?.IsCompleted ?? true;
        public bool FinishedWithSuccess() => Finished() && _success;
        public void Dispose() // TODO
        {
        }
    }
    public class ServerRequestTask<TResponse>: IDisposable
    where TResponse : class
    {
        public TResponse Result => _inner.Result;
        public string Log => _inner.Log;

        private readonly ServerRequestTask<TResponse, EmptyPayload> _inner;
        public ServerRequestTask(AiGateway gateway, RequestType type)
        {
            _inner = new ServerRequestTask<TResponse, EmptyPayload>(gateway, type, new EmptyPayload());
        }

        public bool Finished() => _inner.Finished();
        public bool FinishedWithSuccess() => _inner.FinishedWithSuccess();
        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
