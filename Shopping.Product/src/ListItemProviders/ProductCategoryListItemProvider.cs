﻿using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Plugins.Common.Base.src.Model;
using ZKWeb.Plugins.Shopping.Product.src.Extensions;
using ZKWeb.Plugins.Shopping.Product.src.Managers;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Plugins.Shopping.Product.src.ListItemProviders {
	/// <summary>
	/// 商品类目的选项列表
	/// </summary>
	public class ProductCategoryListItemProvider : IListItemProvider {
		/// <summary>
		/// 获取选项列表
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetItems() {
			var categoryManager = Application.Ioc.Resolve<ProductCategoryManager>();
			var tree = categoryManager.GetCategoryTree();
			foreach (var node in tree.EnumerateAllNodes().Where(t => t.Value != null)) {
				yield return new ListItem(node.GetFullName(), node.Value.Id.ToString());
			}
		}
	}
}