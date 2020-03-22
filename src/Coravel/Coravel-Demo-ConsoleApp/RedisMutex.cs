using Coravel.Scheduling.Schedule.Interfaces;
using RedLockNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Coravel_Demo_ConsoleApp
{
    public class RedisMutex : IMutex
    {
        private readonly IDistributedLockFactory lockFactory;

        private IRedLock lockObject = null;

        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(5);


        public TimeSpan RetryTime { get; set; } = TimeSpan.FromSeconds(10);


        public RedisMutex(IDistributedLockFactory lockFactory)
        {
            this.lockFactory = lockFactory;
        }


        private object SyncLock = new object();

        public bool TryGetLock(string key, int timeoutMinutes)
        {
            lock (SyncLock)
            {
                this.lockObject = lockFactory.CreateLock(
                    key, 
                    TimeSpan.FromMinutes(timeoutMinutes),
                     this.WaitTime,
                     this.RetryTime
                );
                return this.lockObject.IsAcquired;
            }
        }

        public void Release(string key)
        {
            //lock (SyncLock)
            {
                this.lockObject?.Dispose();
                this.lockObject = null;
            }
        }

        
    }
}
