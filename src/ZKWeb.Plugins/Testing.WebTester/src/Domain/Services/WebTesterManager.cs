﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZKWeb.Plugins.Common.Base.src.Domain.Services.Bases;
using ZKWeb.Plugins.Testing.WebTester.src.Components.TestEventHandlers;
using ZKWeb.Plugins.Testing.WebTester.src.Components.WebTester;
using ZKWeb.Plugins.Testing.WebTester.src.Components.WebTester.Enums;
using ZKWeb.Testing;
using ZKWebStandard.Extensions;
using ZKWebStandard.Ioc;

namespace ZKWeb.Plugins.Testing.WebTester.src.Domain.Services {
	/// <summary>
	/// 网页上运行单元测试使用的管理器
	/// 
	/// 单元测试运行流程
	/// - 检查单元测试线程是否已启动，未启动时启动
	/// - 把需要测试的程序标记为等待运行
	/// - 单元测试线程运行等待运行的测试，并设置测试信息
	///   测试信息以程序集为单位
	/// 
	/// 单元测试的信息全局共享并可以随时获取
	/// 单元测试的信息可手动重置，仅限于状态是"运行完毕"
	/// </summary>
	[ExportMany, SingletonReuse]
	public class WebTesterManager : DomainServiceBase {
		/// <summary>
		/// 测试信息列表
		/// 全局唯一，创建后不应该进行更改
		/// </summary>
		protected IEnumerable<AssemblyTestInfo> Informations { get; set; }
		/// <summary>
		/// 测试信息列表使用的线程锁
		/// </summary>
		protected object InformationsLock { get; set; }
		/// <summary>
		/// 测试线程
		/// </summary>
		protected Thread RunningThread { get; set; }
		/// <summary>
		/// 测试线程使用的锁
		/// 优先级大于InformationsLock
		/// </summary>
		protected object RunningThreadLock { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public WebTesterManager() {
			var testManager = Application.Ioc.Resolve<TestManager>();
			var assemblies = testManager.GetAssembliesForTest();
			Informations = assemblies.Select(a => new AssemblyTestInfo(a)).ToList();
			InformationsLock = new object();
			RunningThread = null;
			RunningThreadLock = new object();
		}

		/// <summary>
		/// 检查测试线程是否已运行，未运行时运行
		/// </summary>
		protected virtual void EnsureRunningThread() {
			if (RunningThread != null && RunningThread.IsAlive) {
				return;
			}
			lock (RunningThreadLock) {
				if (RunningThread != null && RunningThread.IsAlive) {
					return;
				}
				RunningThread = new Thread(RunningThreadBody);
				RunningThread.IsBackground = true;
				RunningThread.Start();
			}
		}

		/// <summary>
		/// 测试线程的运行函数
		/// </summary>
		protected virtual void RunningThreadBody() {
			while (true) {
				// 获取要运行的测试，没有时退出循环
				string assemblyToRun = null;
				lock (RunningThreadLock) {
					lock (InformationsLock) {
						var infoToRun = Informations.FirstOrDefault(info =>
							info.State == AssemblyTestState.WaitingToRun);
						if (infoToRun == null) {
							RunningThread = null;
							return;
						}
						assemblyToRun = infoToRun.AssemblyName;
					}
				}
				// 运行测试
				var testManager = Application.Ioc.Resolve<TestManager>();
				var assemblies = testManager.GetAssembliesForTest();
				var assembly = assemblies.First(a => a.GetName().Name == assemblyToRun);
				var eventHandler = new TestWebEventHandler();
				testManager.RunAssemblyTest(assembly, eventHandler);
			}
		}

		/// <summary>
		/// 获取当前的测试信息
		/// 返回的列表是克隆后的内容
		/// </summary>
		/// <returns></returns>
		public virtual IList<AssemblyTestInfo> GetInformations() {
			return GetInformations(new Dictionary<string, string>());
		}

		/// <summary>
		/// 获取当前的测试信息，只获取差异部分
		/// 返回的列表是克隆后的内容
		/// </summary>
		/// <param name="updateTimes">{ 程序集: 更新时间 }</param>
		/// <returns></returns>
		public virtual IList<AssemblyTestInfo> GetInformations(
			IDictionary<string, string> updateTimes) {
			var result = new List<AssemblyTestInfo>();
			lock (InformationsLock) {
				foreach (var information in Informations) {
					var clientUpdateTime = updateTimes.GetOrDefault(information.AssemblyName);
					if (clientUpdateTime != information.UpdateTime) {
						result.Add(information.CloneByJson());
					}
				}
			}
			return result;
		}

		/// <summary>
		/// 重置所有测试信息
		/// </summary>
		public virtual void ResetInformations() {
			lock (InformationsLock) {
				foreach (var information in Informations) {
					if (information.Resetable()) {
						information.Reset();
					}
				}
			}
		}

		/// <summary>
		/// 更新指定的测试信息
		/// </summary>
		/// <param name="assemblyName">程序集名称</param>
		/// <param name="action">更新函数</param>
		public virtual void UpdateInformation(string assemblyName, Action<AssemblyTestInfo> action) {
			lock (InformationsLock) {
				foreach (var information in Informations) {
					if (information.AssemblyName == assemblyName) {
						action(information);
					}
				}
			}
		}

		/// <summary>
		/// 后台运行指定的测试
		/// </summary>
		/// <param name="assemblyName">程序集名称</param>
		public virtual void RunAssemblyTestBackground(string assemblyName) {
			lock (InformationsLock) {
				foreach (var information in Informations) {
					if (information.AssemblyName == assemblyName) {
						information.TryWaitingToRun();
					}
				}
			}
			EnsureRunningThread();
		}

		/// <summary>
		/// 后台运行所有测试
		/// </summary>
		public virtual void RunAllAssemblyTestBackground() {
			lock (InformationsLock) {
				foreach (var information in Informations) {
					information.TryWaitingToRun();
				}
			}
			EnsureRunningThread();
		}
	}
}
