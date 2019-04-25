// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	partial class StoreService
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Called when the store initialize operation has been initiated. Default implementation raises <see cref="InitializeInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnInitializeCompleted(FetchCompletedEventArgs)"/>
		protected virtual void OnInitializeInitiated(AsyncInitiatedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnInitializeInitiated));
				_text.Append(": UnityIAP v");
				_text.Append(StandardPurchasingModule.k_PackageVersion);
				_text.Append('.');

				_console.TraceEvent(TraceEventType.Start, args.OperationId, _text.ToString());

				InitializeInitiated?.Invoke(this, args);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store initialization has succeeded. Default implementation raises <see cref="InitializeCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnInitializeInitiated(AsyncInitiatedEventArgs)"/>
		protected virtual void OnInitializeCompleted(FetchCompletedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnInitializeCompleted));

				if (args.IsFailed)
				{
					_text.Append(", failReason: ");

					if (args.ErrorCode != null)
					{
						_text.Append(args.ErrorCode.ToString());
					}
					else
					{
						_text.Append(args.Error.GetType().ToString());
					}

					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, args.OperationId, _text.ToString());

				InitializeCompleted?.Invoke(this, args);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated. Default implementation raises <see cref="FetchInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnFetchCompleted(FetchCompletedEventArgs)"/>
		protected virtual void OnFetchInitiated(AsyncInitiatedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_console.TraceEvent(TraceEventType.Start, args.OperationId, nameof(OnFetchInitiated));

				FetchInitiated?.Invoke(this, args);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store fetch has succeeded. Default implementation raises <see cref="FetchCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnFetchInitiated(AsyncInitiatedEventArgs)"/>
		protected virtual void OnFetchCompleted(FetchCompletedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnFetchCompleted));

				if (args.IsFailed)
				{
					_text.Append(", failReason: ");

					if (args.ErrorCode != null)
					{
						_text.Append(args.ErrorCode.ToString());
					}
					else
					{
						_text.Append(args.Error.GetType().ToString());
					}

					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, args.OperationId, _text.ToString());

				FetchCompleted?.Invoke(this, args);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the restore operation has been initiated.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnRestoreCompleted(AsyncCompletedEventArgs)"/>
		protected virtual void OnRestoreInitiated(AsyncInitiatedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_console.TraceEvent(TraceEventType.Start, args.OperationId, nameof(OnRestoreInitiated));

				RestoreInitiated?.Invoke(this, args);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store fetch has succeeded. Default implementation raises <see cref="FetchCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnRestoreInitiated(AsyncInitiatedEventArgs)"/>
		protected virtual void OnRestoreCompleted(AsyncCompletedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnRestoreCompleted));

				if (args.IsFailed)
				{
					_text.Append(", failReason: ");
					_text.Append(args.Error.GetType().ToString());
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, args.OperationId, _text.ToString());

				RestoreCompleted?.Invoke(this, args);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated. Default implementation raises <see cref="PurchaseInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnPurchaseCompleted(PurchaseCompletedEventArgs)"/>
		protected virtual void OnPurchaseInitiated(PurchaseInitiatedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnPurchaseInitiated));
				_text.Append(": ");
				_text.Append(args.ProductId);

				if (args.Restored)
				{
					_text.Append(", auto-restored.");
				}
				else
				{
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Start, args.OperationId, _text.ToString());

				PurchaseInitiated?.Invoke(this, args);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store purchase operation succeded. Default implementation raises <see cref="PurchaseCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnPurchaseInitiated(PurchaseInitiatedEventArgs)"/>
		protected virtual void OnPurchaseCompleted(PurchaseCompletedEventArgs args)
		{
			Debug.Assert(args != null);

			try
			{
				_text.Clear();
				_text.Append(nameof(OnPurchaseCompleted));
				_text.Append(": ");
				_text.Append(args.ProductId);

				if (args.IsFailed)
				{
					_text.Append(", failReason: ");

					if (args.ErrorCode != null)
					{
						_text.Append(args.ErrorCode.ToString());
					}
					else
					{
						_text.Append(args.Error.GetType().ToString());
					}
				}

				_text.Append(", validationResult: ");
				_text.Append(args.ValidationStatus.ToString());

				if (args.Restored)
				{
					_text.Append(", auto-restored.");
				}
				else
				{
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, args.OperationId, _text.ToString());

				PurchaseCompleted?.Invoke(this, args);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		#endregion

		#region IStoreEvents

		/// <inheritdoc/>
		public event EventHandler<AsyncInitiatedEventArgs> InitializeInitiated;

		/// <inheritdoc/>
		public event EventHandler<FetchCompletedEventArgs> InitializeCompleted;

		/// <inheritdoc/>
		public event EventHandler<AsyncInitiatedEventArgs> FetchInitiated;

		/// <inheritdoc/>
		public event EventHandler<FetchCompletedEventArgs> FetchCompleted;

		/// <inheritdoc/>
		public event EventHandler<AsyncInitiatedEventArgs> RestoreInitiated;

		/// <inheritdoc/>
		public event EventHandler<AsyncCompletedEventArgs> RestoreCompleted;

		/// <inheritdoc/>
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <inheritdoc/>
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		#endregion

		#region implementation
		#endregion
	}
}
