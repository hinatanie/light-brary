using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Exceptions
{
    public class ForbiddenException(string message = "Không có quyền truy cập chức năng này") : Exception(message)
    {
    }
}