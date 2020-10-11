using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.dal.Entities
{
    /// <summary>
    /// Represents a list of images for a given product.
    /// </summary>
    [Owned]
    public class ProductImage
    {
        /// <summary>
        /// Gets the unique ID of the record.
        /// </summary>
        [Key, JsonIgnore]
        public long Id {get; set;}

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        [MaxLength(256), Required]
        public string Url {get; set;} = default!;
    }
}
