using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Domain.Common;

namespace TaskManager.Domain.ValueObjects
{
    public readonly record struct Title
    {
        public string Value { get; }

        private Title(string title) => Value = title;

        public static Result<Title> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<Title>.Failure("Title cannot be empty.");
            }

            if (value.Length > 200)
            {
                return Result<Title>.Failure("Title cannot exceed 200 characters.");
            }

            return Result<Title>.Success(new Title(value));
        }

        public static implicit operator string(Title title) => title.Value;

    }
}
