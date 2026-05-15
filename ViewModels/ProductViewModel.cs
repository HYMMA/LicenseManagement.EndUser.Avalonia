using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public sealed partial class ProductViewModel : ObservableObject, IEquatable<ProductViewModel>
{
    /// <summary>
    /// The Ulid of the product with prefix PRD_.
    /// </summary>
    [ObservableProperty]
    private string? _id;

    /// <summary>
    /// The user-defined name of the product.
    /// </summary>
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private DateTime _created;

    [ObservableProperty]
    private DateTime _updated;

    public static ProductViewModel? FromProductModel(ProductModel? model)
    {
        if (model is null) return null;
        return new ProductViewModel
        {
            Id = model.Id,
            Name = model.Name,
            Created = model.Created ?? DateTime.MinValue,
            Updated = model.Updated ?? DateTime.MinValue
        };
    }

    public bool Equals(ProductViewModel? other) =>
        other is not null && string.Equals(Id, other.Id, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as ProductViewModel);

    public override int GetHashCode() => Id is null ? 0 : StringComparer.Ordinal.GetHashCode(Id);
}
