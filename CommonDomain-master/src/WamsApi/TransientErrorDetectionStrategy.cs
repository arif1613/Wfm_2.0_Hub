using System;
using Microsoft.Practices.TransientFaultHandling;

namespace WamsApi
{
    public class TransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            return true;
        }
    }
}
