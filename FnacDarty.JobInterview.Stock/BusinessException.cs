using System;

namespace FnacDarty.JobInterview.Stock
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}