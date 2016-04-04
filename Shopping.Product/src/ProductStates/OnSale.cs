﻿using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Plugins.Shopping.Product.src.Model;

namespace ZKWeb.Plugins.Shopping.Product.src.ProductStates {
	/// <summary>
	/// 上架中
	/// </summary>
	[ExportMany]
	public class OnSale : IProductState {
		/// <summary>
		/// 商品状态
		/// </summary>
		public string State { get { return "OnSale"; } }
	}
}