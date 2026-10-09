using System;
using System.Collections.Generic;
using System.Threading;
using SPAD.neXt.Interfaces.HID;

namespace VirpilLedControls.StaticDecoupling
{
    public class HidDevices : Interfaces.IHidDevices
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool disposed;

        private void CheckDisposeAndThrow()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(HidDevices));
            }
        }

        public IEnumerable<IHidDevice> Enumerate(int vendorId)
        {
            CheckDisposeAndThrow();

            _lock.EnterReadLock();

            try
            {
                CheckDisposeAndThrow();
                return HidLibrary.HidDevices.Enumerate(vendorId);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            _lock.EnterWriteLock();
            disposed = true;
            try
            {
                if (!disposed)
                {
                    disposed = true;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Add a finalizer to ensure cleanup
        ~HidDevices()
        {
            Dispose();
        }
    }
}