// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Store service settings.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	public interface IStoreServiceSettings
	{
		/// <summary>
		/// Gets or sets a <see cref="SourceSwitch"/> instance that controls tracing and debug output for the service.
		/// </summary>
		SourceSwitch TraceSwitch { get; set; }

		/// <summary>
		/// Returns a collection of trace listeners for the service. Read only.
		/// </summary>
		TraceListenerCollection TraceListeners { get; }
	}
}
