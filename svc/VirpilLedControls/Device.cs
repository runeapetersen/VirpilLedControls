using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VirpilLedControls
{
    public class Device
    {
        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public string Id { get; private set; }
        public Device(string id)
        {
            Id = id;
        }

        public Button GetOrAddButton(string id, CancellationToken ct = default)
        {
            _semaphoreSlim.Wait(ct);
            try
            {
                if (_buttons.TryGetValue(id, out var button))
                    return button;
                button = new Button(id);
                _buttons.Add(button.Id, button);

                return button;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}