using System.Collections.Generic;

namespace Project_X.Contracts.Responses
{
    public class ErrorResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
