using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper
{
    public class JoinDetails
    {
        public JoinDetails(string joinKeyword, string tableName, string constraint)
        {
            this.JoinKeyword = joinKeyword;
            this.TableName = tableName;
            this.Constraint = constraint;
        }

        public string JoinKeyword;
        public string TableName;
        public string Constraint;
    }
}
