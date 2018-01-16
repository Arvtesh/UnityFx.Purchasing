// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A yieldable store operation with status information.
	/// </summary>
	/// <seealso cref="IStoreOperation{T}"/>
	public interface IStoreOperation
	{
		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>. Read only.
		/// </summary>
		/// <seealso cref="IsFaulted"/>
		Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has completed successfully, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has аштшырув, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		bool IsCompleted { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has failed for any reason, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="Exception"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsCanceled"/>
		bool IsFaulted { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has been canceled by user, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		bool IsCanceled { get; }
	}
}
