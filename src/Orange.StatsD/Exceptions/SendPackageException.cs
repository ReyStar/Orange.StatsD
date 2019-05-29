using System;

namespace Orange.StatsD.Exceptions
{
    public class SendPackageException : Exception
    {
        public SendPackageException(string message) :base(message)
        {

        }
    }
}
