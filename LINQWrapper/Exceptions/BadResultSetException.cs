using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.Exceptions
{
    /// <summary>
    /// This exception type is thrown when an SQL result set fails to meet the conditions required
    /// of it for further processing. This may indicate SQL that doesn't meet its requirements, or it
    /// may be a failure caused by expected data being missing from the database.
    /// </summary>
    public class BadResultSetException : ApplicationException
    {
        public BadResultSetException(string message) :
            base(message)
        {
        }
    }
}
