using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            var file = value as IFormFile;
            if(file != null &&
                (!_extensions.Contains(file.ContentType.Split("/")[1]) ||
                 !_extensions.Contains(Path.GetExtension(file.FileName).Substring(1))))
            {
                return new ValidationResult($"Accepted File Types: {string.Join(", ", _extensions)}!");
            }
            return ValidationResult.Success;
        }
    }
}
