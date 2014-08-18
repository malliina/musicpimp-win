using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mle.Iap {
    public class ProductInfo {
        public string Name { get; private set; }
        public string ProductId { get; private set; }
        public string FormattedPrice { get; private set; }
        public ProductInfo(string name, string productID, string formattedPrice) {
            Name = name;
            ProductId = productID;
            FormattedPrice = formattedPrice;
        }

    }
}
