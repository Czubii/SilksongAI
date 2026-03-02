using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public class AiClient: IDisposable
    {
        private readonly string _host;
        private readonly int _port;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Task _recieveLoop;

        private CancellationTokenSource _cts;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<byte[]> OnMessageRecieved;

        public AiClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            _tcpClient = new TcpClient();
            try
            {
                await _tcpClient.ConnectAsync(_host, _port).ConfigureAwait(false);
                _stream = _tcpClient.GetStream();

                OnConnect?.Invoke();

                _cts = new CancellationTokenSource();
                _recieveLoop = Task.Run(() => RecieveLoopAsync(_cts.Token));
            }
            catch
            {
                _tcpClient?.Close();
                _tcpClient = null;
                _stream = null;
                throw;
            }
        }

        public async Task SendAsync(byte[] bytes)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected!");

            byte[] lengthPrefix = BitConverter.GetBytes(bytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthPrefix); // now big-endian

            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public async Task RecieveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {

                    byte[] legthBuffer = await ReadExactAsync(4, token);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(legthBuffer); // now big-endian

                    int length = BitConverter.ToInt32(legthBuffer, 0);

                    var message = new byte[length];
                    message = await ReadExactAsync(length, token);

                    OnMessageRecieved.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                HandleDisconnect(ex);
            }

        }
        private async Task<byte[]> ReadExactAsync(int size, CancellationToken token)
        {
            byte[] buffer = new byte[size];
            int totalRead = 0;

            while (totalRead < size)
            {
                int read = await _stream.ReadAsync(buffer, totalRead, size - totalRead, token);

                if (read == 0)
                    throw new Exception("Server closed connection");

                totalRead += read;
            }

            return buffer;
        }

        private void HandleDisconnect(Exception e)
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();

            OnDisconnect?.Invoke();
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}
