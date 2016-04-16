﻿using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Localize.Interfaces;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Plugins.Shopping.Logistics.src.Translates {
	/// <summary>
	/// 日本语翻译
	/// </summary>
	[ExportMany, SingletonReuse]
	public class ja_JP : ITranslateProvider {
		private static HashSet<string> Codes = new HashSet<string>() { "ja-JP" };
		private static Dictionary<string, string> Translates = new Dictionary<string, string>()
		{
			{ "Logistics", "物流" },
			{ "LogisticsManage", "物流管理" },
			{ "Logistics management", "物流管理" },
			{ "LogisticsPriceRules", "送料ルール" },
			{ "Logistics cost is determined by the following settings, match order is from top to bottom",
				"送料は以下のルールをもって計算されます，照合順番は上から下へ" },
			{ "LogisticsType", "物流タイプ" },
			{ "Express", "速達郵便" },
			{ "SurfaceMail", "船便" }
		};

		public bool CanTranslate(string code) {
			return Codes.Contains(code);
		}

		public string Translate(string text) {
			return Translates.GetOrDefault(text);
		}
	}
}