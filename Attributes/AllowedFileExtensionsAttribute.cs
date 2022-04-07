using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Project_X.Attributes
{
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedFileExtensionsAttribute(params string[] extensions)
        {
            _extensions = new string[extensions.Length];
            extensions.CopyTo(_extensions, 0);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file &&
                _extensions.Contains(file.ContentType.Split("/")[1]) &&
                _extensions.Contains(Path.GetExtension(file.FileName)[1..]))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"Accepted File Types: {string.Join(", ", _extensions)}!");
        }
    }
}
