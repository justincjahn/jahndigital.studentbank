using System;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models
{
    public class NewStockRequest
    {
        /// <summary>
        ///     Unique symbol of the stock.
        /// </summary>
        public string Symbol { get; set; } = default!;

        /// <summary>
        ///     Name of the company
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        ///     The description of the stock.
        /// </summary>
        public string? RawDescription { get; set; }

        /// <summary>
        ///     The current value of the stock.
        /// </summary>
        public Money CurrentValue { get; set; } = default!;
    }
}
