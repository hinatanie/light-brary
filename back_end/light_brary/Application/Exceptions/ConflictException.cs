using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message = "Dữ liệu bị xung đột") : base(message) { }
    }
}