using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestAttributes
{
    public class TestOrderByModifierAttribute : OrderByModifierAttribute
    {
        #region OrderByModifierAttribute Members

        public override string GetOrderByExpression(string fieldName)
        {
            return "ModifierFunction(" + fieldName + ")";
        }

        #endregion
    }
}
