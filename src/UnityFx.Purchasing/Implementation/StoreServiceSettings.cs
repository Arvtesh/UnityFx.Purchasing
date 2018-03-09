// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	internal class StoreServiceSettings : IStoreServiceSettings
	{
		#region data

		private readonly TraceSource _console;
		private readonly StoreListener _storeListener;

		#endregion

		#region interface

		public StoreServiceSettings(TraceSource traceSource, StoreListener storeListener)
		{
			_console = traceSource;
			_storeListener = storeListener;
		}

		#endregion

		#region IStoreServiceSettings

		public SourceSwitch TraceSwitch { get => _console.Switch; set => _console.Switch = value; }
		public TraceListenerCollection TraceListeners => _console.Listeners;
		public int MaxNumberOfPendingPurchases { get => _storeListener.MaxNumberOfPendingPurchases; set => _storeListener.MaxNumberOfPendingPurchases = value; }

		#endregion
	}
}
