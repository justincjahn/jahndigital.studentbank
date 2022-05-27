using System.Collections.ObjectModel;
using JahnDigital.StudentBank.Domain.Common;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a purchase for students.  One purchase can have multiple line items.
/// </summary>
public class StudentPurchase : AuditableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Get or set the ID number for the student.
    /// </summary>
    public long StudentId { get; set; }

    /// <summary>
    ///     Get or set the student associated with this purchase.
    /// </summary>
    public Student Student { get; set; } = default!;

    /// <summary>
    ///     Get the raw (database stored) total cost of the purchase.
    /// </summary>
    public long RawTotalCost { get; private set; }

    /// <summary>
    ///     Get or set the total cost of the purchase.
    /// </summary>
    public Money TotalCost
    {
        get => Money.FromDatabase(RawTotalCost);
        set => RawTotalCost = value.DatabaseAmount;
    }

    /// <summary>
    ///     Get or set the status of the purchase.
    /// </summary>
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Placed;

    /// <summary>
    ///     Mutable list of items on this purchase.
    /// </summary>
    private readonly ICollection<StudentPurchaseItem> _purchaseItems = new HashSet<StudentPurchaseItem>();

    /// <summary>
    ///     Get a list of line items on this purchase.
    /// </summary>
    public IReadOnlyCollection<StudentPurchaseItem> Items
    {
        get => new ReadOnlyCollection<StudentPurchaseItem>(_purchaseItems.ToList());
    }

    /// <summary>
    /// Adds an item to the purchase.
    /// </summary>
    /// <param name="item"></param>
    public void AddPurchaseItem(StudentPurchaseItem item)
    {
        _purchaseItems.Add(item);
        TotalCost += item.TotalPurchasePrice;
    }

    /// <summary>
    /// Removes an item from the purchase.
    /// </summary>
    /// <param name="item"></param>
    public void RemovePurchaseItem(StudentPurchaseItem item)
    {
        var wasRemoved = _purchaseItems.Remove(item);

        if (wasRemoved)
        {
            TotalCost -= item.TotalPurchasePrice;
        }
    }

    /// <summary>
    /// Updates an existing item on the purchase.
    /// </summary>
    /// <param name="item"></param>
    public void UpdatePurchaseItem(StudentPurchaseItem item)
    {
        RemovePurchaseItem(item);
        AddPurchaseItem(item);
    }
}
