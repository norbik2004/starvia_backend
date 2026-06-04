using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Consts;

namespace tr_core.Helpers
{
    public abstract class BaseHelpers
    {
        public static void ValidateQueryParamsDates(DateTime? CreatedBefore, DateTime? CreatedAfter)
        {
            if(CreatedAfter != null && CreatedBefore != null)
            {
                if (CreatedAfter >= CreatedBefore)
                    throw new ArgumentException("CreatedAfter date cannot be greater than CreatedBefore date");
                else if (CreatedBefore <= CreatedAfter)
                    throw new ArgumentException("CreatedBefore date cannot be less than CreatedAfter date");
            }
        }
    }
}
