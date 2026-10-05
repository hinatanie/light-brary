using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message = "Dữ liệu đầu vào không hợp lệ") : base(message) { }
    }
}