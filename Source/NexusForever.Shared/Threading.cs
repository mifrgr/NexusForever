using System;
using System.Threading;

#nullable enable

namespace NexusForever.Shared
{
    /// <summary>
    /// Disposable class to enter EnterReadLock with using statement 
    /// </summary>
    public struct DisposableReadLock : IDisposable
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
            this.readerLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Disposable class to enter EnterReadLock with using statement 
    /// </summary>
    public struct DisposableWriteLock : IDisposable
    {
        /// <summary>
        /// Managed lock
        /// </summary>
        private readonly ReaderWriterLockSlim writerLock;

        /// <summary>
        /// Create the lock and enter read mode
        /// </summary>
        public DisposableWriteLock(ReaderWriterLockSlim writerLock)
        {
            this.writerLock = writerLock;
            this.writerLock.EnterWriteLock();
        }

        /// <summary>
        /// Exit read mode
        /// </summary>
        public void Dispose()
        {
            this.writerLock.ExitWriteLock();
        }
    }

    public static partial class ThreadingExtensions
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
