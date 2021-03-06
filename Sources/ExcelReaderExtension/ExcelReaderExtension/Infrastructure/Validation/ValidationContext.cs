﻿using ExcelReaderExtension.Infrastructure.Interface;
using ExcelReaderExtension.Infrastructure.Model;
using System;
using System.Linq.Expressions;

namespace ExcelReaderExtension.Infrastructure.Validation
{
    internal class ValidationContext<T>
    {
        public IValidationRule Rule { get; set; }

        public Expression<Func<CellResource, string>> DefaultMessage { get; set; }

        public Expression<Func<CellResource, T, string>> Message { get; set; }

        public ValidationContext()
        {
            DefaultMessage = null;
            Message = null;
        }
    }
}