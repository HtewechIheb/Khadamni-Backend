using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Project_X.Attributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxFileSize)
            {
                return new ValidationResult(string.Format("Maximum File Size Is {0:F2} MB!", _maxFileSize / 1024 / 1024));
            }
            return ValidationResult.Success;
        }
    }
}
