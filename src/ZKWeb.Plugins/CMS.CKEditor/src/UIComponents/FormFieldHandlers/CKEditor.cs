﻿using Newtonsoft.Json;
using System.Collections.Generic;
using ZKWebStandard.Ioc;
using ZKWeb.Templating;
using ZKWebStandard.Utils;
using ZKWeb.Plugins.Common.Base.src.UIComponents.Forms.Attributes;
using ZKWeb.Plugins.Common.Base.src.UIComponents.Forms.Interfaces;
using ZKWeb.Plugins.Common.Base.src.UIComponents.Forms;
using System;
using ZKWeb.Plugins.Common.Base.src.UIComponents.Forms.Extensions;
#if !NETCORE
using Ganss.XSS;
#endif

namespace ZKWeb.Plugins.CMS.CKEditor.src.UIComponents.FormFieldHandlers {
	/// <summary>
	/// CKEditor编辑器
	/// </summary>
	[ExportMany(ContractKey = typeof(RichTextEditorAttribute)), SingletonReuse]
	public class CKEditor : IFormFieldHandler {
		/// <summary>
		/// 获取表单字段的html
		/// </summary>
		public string Build(FormField field, IDictionary<string, string> htmlAttributes) {
			var attribute = (RichTextEditorAttribute)field.Attribute;
			var templateManager = Application.Ioc.Resolve<TemplateManager>();
			var ckeditor = templateManager.RenderTemplate("cms.ckeditor/tmpl.form.ckeditor.html", new {
				name = attribute.Name,
				value = (field.Value ?? "").ToString(),
				attributes = htmlAttributes,
				ckeditorConfig = JsonConvert.SerializeObject(attribute.Config)
			});
			return field.WrapFieldHtml(htmlAttributes, ckeditor);
		}

		/// <summary>
		/// 解析提交的字段的值
		/// </summary>
		public object Parse(FormField field, IList<string> values) {
			// 解码提交回来的Html内容
			var html = HttpUtils.HtmlDecode(values[0]);
			// 过滤不安全的内容
#if NETCORE
			throw new NotSupportedException("HtmlSanitizer is unsupported on .Net Core");
#else
			var sanitizer = new HtmlSanitizer();
			sanitizer.RemovingAttribute += (e, args) => {
				// 允许base64图片，因为Uri有65520的长度限制，只能使用事件允许
				if (args.Attribute.Key == "src" && args.Attribute.Value.StartsWith("data:")) {
					args.Cancel = true;
				}
			};
			html = sanitizer.Sanitize(html);
			return html;
#endif
		}
	}
}
