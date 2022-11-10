using System.Collections.Generic;
using System.Linq;

namespace Skewly.Common
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();

        public ValidationResult()
        {

        }

        public ValidationResult(string error) : this()
        {
            Errors.Add(error);
        }
    }
}
