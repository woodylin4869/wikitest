using System;
using System.Collections.Concurrent;
using System.Threading;

namespace H1_ThirdPartyWalletAPI.Model
{
    public class RequestProcessingFlag
    {
        private int _count = 0;
        private long _unixSecond = 0;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
            Interlocked.Exchange(ref _unixSecond, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }

        public int Count => _count;

        public long UnixSecond => _unixSecond;
    }
}
