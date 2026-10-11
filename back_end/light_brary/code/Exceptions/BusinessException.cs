using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Exceptions
{
    public class BusinessException(string message = "Dữ liệu đầu vào không hợp lệ") : Exception(message)
    {
    }
}