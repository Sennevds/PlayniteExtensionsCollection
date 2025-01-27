﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class WishlistItemCache
    {
        public string Name { get; set; }
        public string StoreId { get; set; }
        public double? SubId { get; set; }
        public double? PriceOriginal { get; set; }
        public double? PriceFinal { get; set; }
        public string Currency { get; set; }
        public double DiscountPercent { get; set; }
        public bool IsDiscounted { get; set; }
        public SteamWishlistItem WishlistItem { get; set; }
        public string BannerImagePath { get; set; } = null;
    }
}