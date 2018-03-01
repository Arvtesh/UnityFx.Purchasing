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
		/// <value>Multilevel switch to control tracing and debug output.</value>
		SourceSwitch TraceSwitch { get; set; }

		/// <summary>
		/// Gets a collection of trace listeners for the service.
		/// </summary>
		/// <value>A collection of <see cref="TraceListener"/> objects.</value>
		TraceListenerCollection TraceListeners { get; }

		/// <summary>
		/// Gets or sets maximum number of concurrent purchases. Default value is <c>1</c>.
		/// </summary>
		/// <value>Maximum number of concurrent purchases.</value>
		int MaxNumberOfPendingPurchases { get; set; }
	}
}
