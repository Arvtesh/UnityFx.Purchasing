﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store operation.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public interface IStoreOperationInfo
	{
		/// <summary>
		/// Gets identifier of the corresponding operation.
		/// </summary>
		/// <value>An unique operation identifier.</value>
		int OperationId { get; }

		/// <summary>
		/// Gets user-defined data assosiated with the operation (if any).
		/// </summary>
		/// <value>User-defined data.</value>
		object UserState { get; }
	}
}
