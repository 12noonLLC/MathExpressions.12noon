using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

/// <summary>
/// This interface and class encapsulates the implementation of the
/// INotifyPropertyChanged interface in the variables themselves.
/// </summary>
/// <see cref="https://msdn.microsoft.com/en-us/magazine/mt736453.aspx" />
namespace Shared
{
	public interface IRaisePropertyChanged : INotifyPropertyChanged
	{
		void RaisePropertyChanged(string propertyName);
	}


	/// <summary>
	/// This solution for INotifyPropertyChanged moves the "set" code and the
	/// responsibility for raising the PropertyChanged event into a separate class.
	/// </summary>
	/// <example>
	/// public class ViewModel : IRaisePropertyChanged
	/// {
	///	// Do this for each member that needs to raise PropertyChanged
	///	private readonly NotifyProperty<int> _count;
	///	public int Count
	///	{
	///		get => _count.Value;
	///		set => _count.Value = value;
	///	}
	///
	/// 	public ViewModel()
	/// 	{
	/// 		_count = new NotifyProperty<int>(this, nameof(Count), initialValue: 0);
	/// 	}
	/// 
	///	// Boilerplate code
	/// 	#region IRaisePropertyChanged
	/// 
	/// 	#region INotifyPropertyChanged
	/// 
	/// 	public event PropertyChangedEventHandler PropertyChanged;
	/// 
	/// 	#endregion INotifyPropertyChanged
	/// 
	/// 	public void RaisePropertyChanged(string propertyName)
	/// 	{
	/// 		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	/// 	}
	/// 
	/// 	#endregion IRaisePropertyChanged
	/// }
	/// </example>
	/// <typeparam name="T">The type of the stored value</typeparam>
	public class NotifyProperty<T>
	{
		private readonly IRaisePropertyChanged _owner;

		internal string Name { get; private set; }

		private T _value;
		public T Value
		{
			get => _value;
			set
			{
				if (!EqualityComparer<T>.Default.Equals(value, _value))
				{
					_value = value;

					_owner.RaisePropertyChanged(Name);
				}
			}
		}

		public NotifyProperty(IRaisePropertyChanged owner, string name, T initialValue)
		{
			Debug.Assert(!String.IsNullOrWhiteSpace(name));

			_owner = owner;
			Name = name;
			_value = initialValue;
		}
	}
}
