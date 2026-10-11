using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Exceptions
{
    public class UnauthorizedException(string message = "Thông tin đăng nhập không chính xác") : Exception(message)
    {
    }
}