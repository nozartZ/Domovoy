﻿using System.ComponentModel.DataAnnotations;

namespace Domovoy_Models
{
    public class ApplicationType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

    }
}
