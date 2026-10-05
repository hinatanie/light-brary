using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Không có quyền truy cập chức năng này") : base(message) { }
    }
}