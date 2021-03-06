﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace Bangazon.Models
{
    public class ProductRating
    {
        [Key]
        public int ProductRatingId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public ApplicationUser User { get; set; }
        public Product Product { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Display(Name = "Rating")]
        [Range(1, 5)]
        public int? Rating { get; set; }
    }
}
