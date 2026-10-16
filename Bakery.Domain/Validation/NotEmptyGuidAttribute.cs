using System.ComponentModel.DataAnnotations;

namespace Bakery.Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
        : base("The {0} field must be a non-empty GUID.")
    {
    }

    public override bool IsValid(object? value) =>
        value is Guid id && id != Guid.Empty;
}
