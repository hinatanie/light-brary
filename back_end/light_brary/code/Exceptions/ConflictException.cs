using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Exceptions
{
    public class ConflictException(string message = "Dữ liệu bị xung đột") : Exception(message)
    {
    }
}