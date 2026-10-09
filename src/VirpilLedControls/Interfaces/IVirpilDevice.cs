using System;
using VirpilLedControls.Model.Commands;

namespace VirpilLedControls.Interfaces
{
    public interface IVirpilDevice : IDisposable
    {
        uint Pid { get; }
        void SetColors(LedCommand ledCommand);
    }
}