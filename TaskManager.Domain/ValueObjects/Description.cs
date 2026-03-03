using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Domain.Common;

namespace TaskManager.Domain.ValueObjects
{
    public readonly record struct Description
    {
        public string Value { get; }

        private Description(string description) => Value = description;


        public static Result<Description> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Result<Description>.Success(new Description(string.Empty));
            }

            if (value.Length > 2000)
            {
                return Result<Description>.Failure("Title cannot exceed 200 characters.");
            }

            return Result<Description>.Success(new Description(value));
        }

        public static implicit operator string(Description description) => description.Value;
    }
}
