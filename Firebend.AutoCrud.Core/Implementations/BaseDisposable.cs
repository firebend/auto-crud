using System;

namespace Firebend.AutoCrud.Core.Implementations
{
    public class BaseDisposable : IDisposable
    {
        private bool _disposed;

        ~BaseDisposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManagedObjects()
        {

        }

        protected virtual void DisposeUnmanagedObjectsAndAssignNull()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeManagedObjects();
            }

            DisposeUnmanagedObjectsAndAssignNull();

            _disposed = true;
        }
    }
}
