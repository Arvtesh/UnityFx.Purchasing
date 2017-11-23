// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents a fetch/initialize operation.
	/// </summary>
	/// <remarks>
	/// The class stored all transaction-related data. The transaction begins when the class instance is created
	/// and ends on <see cref="Dispose"/> call.
	/// </remarks>
	internal class InitializeOperation : IDisposable
	{
		#region data

		private const int _traceEventId = (int)StoreService.TraceEventId.Initialize;

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly IPurchasingModule _purchasingModule;
		private readonly IStoreListener _storeListener;

		private TaskCompletionSource<object> _fetchOpCs;
		private bool _disposed;
		private bool _success;

		#endregion

		#region interface

		public Task Task => _fetchOpCs.Task;

		public InitializeOperation(StoreService storeService, TraceSource console, IPurchasingModule purchasingModule, IStoreListener storeListener)
		{
			Debug.Assert(storeService != null);

			_storeService = storeService;
			_console = console;
			_purchasingModule = purchasingModule;
			_storeListener = storeListener;
			_fetchOpCs = new TaskCompletionSource<object>();

			_console.TraceEvent(TraceEventType.Start, _traceEventId, StoreService.GetEventName(_traceEventId));
		}

		public Task Execute(StoreConfig config)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(config != null);

			var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
			configurationBuilder.AddProducts(config.Products);

			UnityPurchasing.Initialize(_storeListener, configurationBuilder);
			return _fetchOpCs.Task;
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(controller != null);

			_success = true;
			_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "OnInitialized");

			try
			{
				_fetchOpCs.SetResult(null);
				_storeService.InvokeInitializeCompleted(controller.products);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventId, e);
				_fetchOpCs.SetException(e);
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.Assert(!_disposed);

			_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "OnInitializeFailed: " + error);

			try
			{
				_fetchOpCs.SetException(new StoreFetchException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventId, e);
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_fetchOpCs = null;

				if (_success)
				{
					_console.TraceEvent(TraceEventType.Stop, _traceEventId, StoreService.GetEventName(_traceEventId) + " completed");
				}
				else
				{
					_console.TraceEvent(TraceEventType.Stop, _traceEventId, StoreService.GetEventName(_traceEventId) + " failed");
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
