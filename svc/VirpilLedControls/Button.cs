using System;
using System.Threading;
using System.Threading.Tasks;

namespace VirpilLedControls
{
    public class Button
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public string Id { get; private set; }

        public Button(string id)
        {
            Id = id;
        }

        private CancellationTokenSource _localCancellationTokenSource;
        private Task _runningTask;

        public void UpdateActiveTask<T>(Action<T, CancellationToken> ac, CancellationToken ct, T def)
        {
            _semaphoreSlim.Wait(ct);
            try {
                if (_runningTask != null) 
                {
                    _localCancellationTokenSource.Cancel();
                    try
                    {
                        bool completed = _runningTask.Wait(200, ct);
                        if (!completed && _runningTask.Exception != null)
                            _runningTask.Exception.Handle(ex => true);
                    }
                    catch (Exception) { /* Expected during cancellation */ }
                    _runningTask = null;
                }

                _localCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var currentToken = _localCancellationTokenSource.Token;
                _runningTask = Task.Run(() => ac(def, ct), currentToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}