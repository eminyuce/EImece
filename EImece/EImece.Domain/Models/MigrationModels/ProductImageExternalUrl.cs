﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.MigrationModels
{
    public class ProductImageExternalUrl
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageFullPath { get; set; }
        public string EntityImageType { get; set; }
    }
}
