using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.Exceptions
{
    public class IncompleteQueryException : ApplicationException
    {
        public IncompleteQueryException() :
            base("Incomplete Query")
        { }
    }
}
