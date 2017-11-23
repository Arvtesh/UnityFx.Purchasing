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
	internal class FetchOperation : IDisposable
	{
		#region data

		private const int _traceEventId = (int)StoreService.TraceEventId.Fetch;

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly IStoreController _storeController;

		private TaskCompletionSource<object> _fetchOpCs;
		private bool _disposed;
		private bool _success;

		#endregion

		#region interface

		public Task Task => _fetchOpCs.Task;

		public FetchOperation(StoreService storeService, TraceSource console, IStoreController storeController)
		{
			Debug.Assert(storeService != null);

			_storeService = storeService;
			_console = console;
			_storeController = storeController;
			_fetchOpCs = new TaskCompletionSource<object>();

			_console.TraceEvent(TraceEventType.Start, _traceEventId, StoreService.GetEventName(_traceEventId));
			_storeService.InvokeFetchInitiated();
		}

		public Task Execute(StoreConfig config)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(config != null);

			var products = new HashSet<ProductDefinition>(config.Products);
			_storeController.FetchAdditionalProducts(products, OnFetch, OnFetchFailed);
			return _fetchOpCs.Task;
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

		private void OnFetch()
		{
			if (!_storeService.IsDisposed)
			{
				_success = true;
				_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "OnFetch");

				try
				{
					_fetchOpCs.SetResult(null);
					_storeService.InvokeFetchCompleted(_storeController.products);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventId, e);
					_fetchOpCs.SetException(e);
				}
			}
		}

		private void OnFetchFailed(InitializationFailureReason error)
		{
			if (!_storeService.IsDisposed)
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "OnFetchFailed: " + error);

				try
				{
					_fetchOpCs.SetException(new StoreFetchException(error));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventId, e);
				}
			}
		}

		#endregion
	}
}
