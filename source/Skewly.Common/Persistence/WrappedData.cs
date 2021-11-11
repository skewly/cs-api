﻿namespace Skewtech.Common.Persistence
{
    public class WrappedData<T> where T : class, new()
    {
        public string Id { get; set; }
        public long? Version { get; set; }
        public T Data { get; set; }
    }
}
