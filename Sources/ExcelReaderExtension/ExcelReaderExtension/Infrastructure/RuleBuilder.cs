﻿using ExcelReaderExtension.Exceptions;
using ExcelReaderExtension.Infrastructure.Interface;
using ExcelReaderExtension.Infrastructure.Model;
using ExcelReaderExtension.Infrastructure.Validation;
using ExcelReaderExtension.Infrastructure.Validation.Rules;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExcelReaderExtension.Infrastructure
{
    public class RuleBuilder<T> : IRuleBuilder<T>
    {
        private readonly Cell<T> model;
        private readonly IParse<T> parse;
        private readonly ExcelRangeBase excelRange;
        private readonly List<ValidationContext<T>> validations;

        public RuleBuilder(ExcelRangeBase excelRange, IParse<T> parse)
        {
            this.excelRange = excelRange;
            this.parse = parse;

            validations = new List<ValidationContext<T>>();

            model = new Cell<T>
            {
                Row = excelRange.Rows,
                Column = excelRange.Columns,
                Address = excelRange.Address,
                WorksheetName = excelRange.Worksheet.Name
            };
        }

        public T Get()
        {
            foreach (var validation in validations)
            {
                if (validation.Rule.IsValid() == false)
                {
                    var function = validation.Message.Compile();
                    throw new ValidationErrorException(function(model));
                }
            }

            return parse.Get();
        }

        public IRuleBuilder<T> WithMessage(Expression<Func<Cell<T>, string>> message)
        {
            if (validations.Any())
            {
                validations.Last().Message = message;
            }
            return this;
        }

        #region Rules

        public IRuleBuilder<T> Contains(params T[] sources)
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new DefaultExpressionRule(() => sources.Contains(parse.Get())),
                Message = cell => $"{cell.Address} is not contains."
            });

            return this;
        }

        public IRuleBuilder<T> NotNull()
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new NotNullRule(excelRange.Value),
                Message = cell => $"{cell.Address} is not null."
            });

            return this;
        }

        public IRuleBuilder<T> NumericOnly()
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new NumericOnlyRule(excelRange.Value),
                Message = cell => $"{cell.Address} is not numeric."
            });

            return this;
        }

        public IRuleBuilder<T> DecimalOnly()
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new DecimalOnlyRule(excelRange.Value),
                Message = detail => $"{detail.Address} is not decimal."
            });

            return this;
        }

        public IRuleBuilder<T> Must(Expression<Func<T, bool>> condition)
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new ExpressionRule<T>(parse.Get(), condition),
                Message = cell => $"{cell.Address} is invalid."
            });

            return this;
        }

        public IRuleBuilder<T> Must(Expression<Func<bool>> condition)
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new DefaultExpressionRule(condition),
                Message = cell => $"{cell.Address} is invalid."
            });

            return this;
        }

        public IRuleBuilder<T> Must(IValidationRule validationRule)
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = validationRule,
                Message = cell => $"{cell.Address} is invalid rule."
            });

            return this;
        }

        public IRuleBuilder<T> NotEmpty()
        {
            validations.Add(new ValidationContext<T>
            {
                Rule = new NotEmptyRule(excelRange.Value),
                Message = cell => $"{cell.Address} is not empty."
            });

            return this;
        }

        #endregion Rules
    }
}