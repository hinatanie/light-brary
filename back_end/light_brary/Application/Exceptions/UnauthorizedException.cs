using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Thông tin đăng nhập không chính xác") : base(message) { }
    }
}