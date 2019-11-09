using System;
using System.Threading;

#nullable enable

namespace NexusForever.Shared
{
    /// <summary>
    /// Disposable class to EnterReadLock with using statement 
    /// </summary>
    public readonly struct DisposableReadLock : IDisposable
    {
        /// <summary>
        /// Managed lock
        /// </summary>
        private readonly ReaderWriterLockSlim readerLock;

        /// <summary>
        /// Create the lock and enter read mode
        /// </summary>
        public DisposableReadLock(ReaderWriterLockSlim readerLock)
        {
            this.readerLock = readerLock;
            this.readerLock.EnterReadLock();
        }

        /// <summary>
        /// Exit read mode
        /// </summary>
        public void Dispose()
        {
            readerLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Disposable class to EnterWriteLock with using statement 
    /// </summary>
    public readonly struct DisposableWriteLock : IDisposable
    {
        /// <summary>
        /// Managed lock
        /// </summary>
        private readonly ReaderWriterLockSlim writerLock;

        /// <summary>
        /// Create the lock and enter write mode
        /// </summary>
        public DisposableWriteLock(ReaderWriterLockSlim writerLock)
        {
            this.writerLock = writerLock;
            this.writerLock.EnterWriteLock();
        }

        /// <summary>
        /// Exit write mode
        /// </summary>
        public void Dispose()
        {
            writerLock.ExitWriteLock();
        }
    }

    public static class ThreadingExtensions
    {
        /// <summary>
        /// Get disposable read lock
        /// </summary>
        public static DisposableReadLock GetReadLock(this ReaderWriterLockSlim readerLock)
        {
            return new DisposableReadLock(readerLock);
        }

        /// <summary>
        /// Get disposable write lock
        /// </summary>
        public static DisposableWriteLock GetWriteLock(this ReaderWriterLockSlim writerLock)
        {
            return new DisposableWriteLock(writerLock);
        }
    }
}
