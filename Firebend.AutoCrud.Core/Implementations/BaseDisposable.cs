using System;

namespace Firebend.AutoCrud.Core.Implementations
{
    public class BaseDisposable : IDisposable
    {
        protected bool Disposed { get; private set; }

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
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeManagedObjects();
            }

            DisposeUnmanagedObjectsAndAssignNull();

            Disposed = true;
        }
    }
}
