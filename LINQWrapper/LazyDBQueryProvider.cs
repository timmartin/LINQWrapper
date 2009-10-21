using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using IQToolkit;

namespace LINQWrapper
{
    public class LazyDBQueryProvider : QueryProvider
    {
        public override string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
