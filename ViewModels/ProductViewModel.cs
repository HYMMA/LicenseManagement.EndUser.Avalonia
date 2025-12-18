using System;
using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class ProductViewModel : BaseViewModel
{
    private string? _id;
    private string? _name;

    public static ProductViewModel? FromProductModel(ProductModel? model)
    {
        if (model == null)
            return null;

        return new ProductViewModel
        {
            Id = model.Id,
            Name = model.Name,
            Created = model.Created ?? DateTime.MinValue,
            Updated = model.Updated ?? DateTime.MinValue
        };
    }

    /// <summary>
    /// The Ulid of the product with prefix PRD_
    /// </summary>
    public string? Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// The user-defined name of the product
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
