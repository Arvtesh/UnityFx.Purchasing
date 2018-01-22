// Copyright (c) Alexander Bogarsukov.
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
		/// Returns identifier of the corresponding operation. Read only.
		/// </summary>
		int OperationId { get; }

		/// <summary>
		/// Returns user-defined data assosiated with the operation (if any). Read only.
		/// </summary>
		object UserState { get; }
	}
}
