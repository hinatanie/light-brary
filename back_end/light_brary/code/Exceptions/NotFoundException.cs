using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Exceptions
{
    public class NotFoundException(string message = "Không tìm thấy dữ liệu") : Exception(message)
    {
    }
}