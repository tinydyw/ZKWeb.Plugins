﻿using DryIoc;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Core;
using ZKWeb.Plugins.Common.Admin.src.AdminApps;
using ZKWeb.Plugins.Common.Admin.src.Database;
using ZKWeb.Plugins.Common.Base.src;
using ZKWeb.Plugins.Common.Base.src.Extensions;
using ZKWeb.Plugins.Common.Base.src.Model;

namespace ZKWeb.Plugins.Common.UserContact.src.AjaxTableCallbacks {
	/// <summary>
	/// 添加以下列到用户管理
	///		电话
	///		手机
	/// </summary>
	[ExportMany]
	public class AddColumnsToUserManageApp : IAjaxTableCallbackFor<User, UserManageApp> {
		public void OnBuildTable(AjaxTableBuilder table, AjaxTableSearchBarBuilder searchBar) { }

		public void OnQuery(AjaxTableSearchRequest request, DatabaseContext context, ref IQueryable<User> query) { }

		public void OnSort(AjaxTableSearchRequest request, DatabaseContext context, ref IQueryable<User> query) { }

		public void OnSelect(AjaxTableSearchRequest request, List<KeyValuePair<User, Dictionary<string, object>>> pairs) {
			var contactManager = Application.Ioc.Resolve<UserContactManager>();
			foreach (var pair in pairs) {
				var contact = contactManager.GetContact(pair.Key.Id);
				pair.Value["Tel"] = contact.Tel;
				pair.Value["Mobile"] = contact.Mobile;
			}
		}

		public void OnResponse(AjaxTableSearchRequest request, AjaxTableSearchResponse response) {
			response.Columns.MoveAfter(response.Columns.AddMemberColumn("Tel"), "Username");
			response.Columns.MoveAfter(response.Columns.AddMemberColumn("Mobile"), "Tel");
		}
	}
}