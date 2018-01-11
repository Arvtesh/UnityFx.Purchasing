// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IObservable{T}"/>.
	/// </summary>
	internal class StoreObservable<T> : IObservable<T>, IObserver<T>
	{
		#region data

		private List<IObserver<T>> _observers;

		#endregion

		#region IObservable

		public void OnNext(T data)
		{
			if (_observers != null)
			{
				lock (_observers)
				{
					foreach (var item in _observers)
					{
						item.OnNext(data);
					}
				}
			}
		}

		public void OnError(Exception error)
		{
			if (_observers != null)
			{
				lock (_observers)
				{
					foreach (var item in _observers)
					{
						item.OnError(error);
					}

					_observers.Clear();
				}
			}
		}

		public void OnCompleted()
		{
			if (_observers != null)
			{
				lock (_observers)
				{
					foreach (var item in _observers)
					{
						item.OnCompleted();
					}

					_observers.Clear();
				}
			}
		}

		#endregion

		#region IObservable

		private class Subscription : IDisposable
		{
			private readonly List<IObserver<T>> _observers;
			private readonly IObserver<T> _observer;

			public Subscription(List<IObserver<T>> observers, IObserver<T> observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				lock (_observers)
				{
					_observers.Remove(_observer);
				}
			}
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			if (_observers == null)
			{
				_observers = new List<IObserver<T>>() { observer };
			}
			else
			{
				lock (_observers)
				{
					_observers.Add(observer);
				}
			}

			return new Subscription(_observers, observer);
		}

		#endregion
	}
}
