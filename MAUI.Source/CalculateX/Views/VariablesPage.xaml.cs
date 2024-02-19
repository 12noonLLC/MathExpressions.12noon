using MathExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CalculateX.Views;

//[QueryProperty(nameof(Variables), ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_NAME)]
public partial class VariablesPage : ContentPage, IQueryAttributable
{
	public record VariableDisplay(string Name, string Value);
	public ObservableCollection<VariableDisplay> Variables { get; init; } = new();


	public VariablesPage()
	{
		InitializeComponent();

		BindingContext = this;

		//WorkspaceID IS NOT set here (by QueryPropertyAttribute)
	}

	private void Variables_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		//WorkspaceID IS set here (by QueryPropertyAttribute)

		Debug.Assert(e.CurrentSelection.Count > 0);

		VariableDisplay entry = (VariableDisplay)e.CurrentSelection[0];
		string variableName = entry.Name;
		string variableValue = entry.Value;
		Shell.Current.GoToAsync($"..?{ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_NAME}={variableName}&{ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_VALUE}={variableValue}");
	}

	///WorkspacePage > WorkspaceViewModel > _workspace.Variables
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLES, out object? value))
		{
			Debug.Assert(value is not null);

			// We cannot use QueryPropertyAttribute because binding
			// has already happened, so we need to bind to an
			// ObservableCollection and--here--load all the vars into it.

			VariableDictionary variables = (VariableDictionary)value;
			foreach (var variable in variables)
			{
				Variables.Add(new VariableDisplay(variable.Key, variable.Value.ToString()));
			}

			//We have to do this for now. We should really move this to the view-model
			// https://stackoverflow.com/q/73755717/4858
			query.Clear();
		}
	}
}
