using CalculateX.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace CalculateX.UnitTests;

[TestClass]
public class TestSynchronize
{
	[ClassInitialize]
	public static void ClassSetup(TestContext _)
	{
#if !MAUI_UNITTESTS
		App.MyAlertService = new AlertServiceUnitTests();
#endif
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
	}


	[TestInitialize]
	public void TestSetup()
	{
	}

	[TestCleanup]
	public void TestTeardown()
	{
	}


	/// <summary>
	/// Test adding a new workspace.
	/// </summary>
	[TestMethod]
	public void TestSynchronizeThisNewWorkspace()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";

		// Arrange
		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
        <workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
            <inputs>
                <key ordinal="1">4+5*3</key>
                <key ordinal="2">(4+5)*3</key>
                <key ordinal="3">sqrt((4+5)*3)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		File.Delete(tempFilePathThis);

		string tempFilePathThat = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThat,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThat);
		File.Delete(tempFilePathThat);

		// Act
		workspacesvmThis.TEST_Synchronize(workspacesvmThat);

		// Assert
		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		string tempFilePathTarget = Path.GetTempFileName();
		File.WriteAllText(tempFilePathTarget,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
        <workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
            <inputs>
                <key ordinal="1">4+5*3</key>
                <key ordinal="2">(4+5)*3</key>
                <key ordinal="3">sqrt((4+5)*3)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmTarget = WorkspacesViewModel.ConstructFromFile(tempFilePathTarget);
		//workspacesvmTarget._workspaces.LastSynchronizedUTC = workspacesvmThis._workspaces.LastSynchronizedUTC;
		File.Delete(tempFilePathTarget);

		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThis);
		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThat);
	}

	/// <summary>
	/// Test adding a new workspace.
	/// </summary>
	[TestMethod]
	public void TestSynchronizeThatNewWorkspace()
	{
		// Arrange
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";

		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		File.Delete(tempFilePathThis);

		string tempFilePathThat = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThat,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
        <workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
            <inputs>
                <key ordinal="1">4+5*3</key>
                <key ordinal="2">(4+5)*3</key>
                <key ordinal="3">sqrt((4+5)*3)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThat);
		File.Delete(tempFilePathThat);

		// Act
		workspacesvmThat.TEST_Synchronize(workspacesvmThis);

		// Assert
		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		string tempFilePathTarget = Path.GetTempFileName();
		File.WriteAllText(tempFilePathTarget,
  """
    <?xml version="1.0" encoding="utf-8"?>
    <workspaces>
        <workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
            <inputs>
                <key ordinal="1">34.5 (23 + 1.5) / 2</key>
                <key ordinal="2">sqrt(4)</key>
                <key ordinal="3">cos(45pi / 180)</key>
            </inputs>
        </workspace>
        <workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
            <inputs>
                <key ordinal="1">4+5*3</key>
                <key ordinal="2">(4+5)*3</key>
                <key ordinal="3">sqrt((4+5)*3)</key>
            </inputs>
        </workspace>
    </workspaces>
    """
		);
		WorkspacesViewModel workspacesvmTarget = WorkspacesViewModel.ConstructFromFile(tempFilePathTarget);
		//workspacesvmTarget._workspaces.LastSynchronizedUTC = workspacesvmThis.LastSynchronizedUTC;
		File.Delete(tempFilePathTarget);

		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThis);
		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThat);
	}

	/// <summary>
	/// Test removing deleted workspaces.
	/// </summary>
	[TestMethod]
	public void TestSynchronizeThisDeleteWorkspace()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";
		const string REMAINING_ID = "73668c98-34de-4c87-9bb6-ba33eafd7874";

		// Arrange
		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		//workspacesvmThat._workspaces.LastSynchronizedUTC = workspacesvmThis.LastSynchronizedUTC;
		File.Delete(tempFilePathThis);

		Assert.AreEqual(2, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.AreEqual(2, workspacesvmThat.TheWorkspaceViewModels.Count);

#if !MAUI_UNITTESTS
		workspacesvmThis.TEST_DeleteSelectedWorkspace();
#else
		workspacesvmThis.TEST_DeleteWorkspace(workspacesvmThis.TheWorkspaceViewModels.Single(wvm => wvm.ID == SELECTED_ID));
#endif

		Assert.AreEqual(1, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.AreEqual(2, workspacesvmThat.TheWorkspaceViewModels.Count);

		// Act
		// synchronize the workspaces with a deleted workspace with a copy of the original workspaces.
		workspacesvmThis.TEST_Synchronize(workspacesvmThat);

		// Assert
		Assert.AreEqual(1, workspacesvmThat.TheWorkspaceViewModels.Count);
		Assert.IsNotNull(workspacesvmThat.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID));
		Assert.AreEqual("That", workspacesvmThat.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID).Name);

		// Only the That workspace should remain after removing deleted workspaces.
		Assert.AreEqual(1, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.IsNotNull(workspacesvmThis.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID));
		Assert.AreEqual("That", workspacesvmThis.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID).Name);

		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		/// Note: We do not check the selected workspace because the view model will handle updating it.
	}

	/// <summary>
	/// Test deleting a new workspace.
	/// </summary>
	[TestMethod]
	public void TestSynchronizeThatDeleteWorkspace()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";
		const string REMAINING_ID = "73668c98-34de-4c87-9bb6-ba33eafd7874";

		// Arrange
		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		//workspacesvmThat._workspaces.LastSynchronizedUTC = workspacesvmThis.LastSynchronizedUTC;
		File.Delete(tempFilePathThis);

		Assert.AreEqual(2, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.AreEqual(2, workspacesvmThat.TheWorkspaceViewModels.Count);

#if !MAUI_UNITTESTS
		workspacesvmThis.TEST_DeleteSelectedWorkspace();
#else
		workspacesvmThis.TEST_DeleteWorkspace(workspacesvmThis.TheWorkspaceViewModels.Single(wvm => wvm.ID == SELECTED_ID));
#endif

		Assert.AreEqual(1, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.AreEqual(2, workspacesvmThat.TheWorkspaceViewModels.Count);

		// Act
		// synchronize the workspaces with a deleted workspace with a copy of the original workspaces.
		workspacesvmThat.TEST_Synchronize(workspacesvmThis);

		// Assert
		// Only the That workspace should remain after removing deleted workspaces.
		Assert.AreEqual(1, workspacesvmThis.TheWorkspaceViewModels.Count);
		Assert.IsNotNull(workspacesvmThis.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID));
		Assert.AreEqual("That", workspacesvmThis.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID).Name);

		Assert.AreEqual(1, workspacesvmThat.TheWorkspaceViewModels.Count);
		Assert.IsNotNull(workspacesvmThat.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID));
		Assert.AreEqual("That", workspacesvmThat.TheWorkspaceViewModels.Single(w => w.ID == REMAINING_ID).Name);

		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		/// Note: We do not check the selected workspace because the view model will handle updating it.
	}

	/// <summary>
	/// Test synchronizing workspace inputs.
	/// </summary>
	[TestMethod]
	public void TestSyncWorkspaceInputs()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";

		// Arrange
		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
				<key ordinal="4">1 + 2</key>
				<key ordinal="5">2 + 3</key>
				<key ordinal="6">3 + 4</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		File.Delete(tempFilePathThis);

		string tempFilePathThat = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThat,
"""
<?xml version="1.0" encoding="utf-8"?>
<workspaces>
	<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
		<inputs>
			<key ordinal="1">34.5 (23 + 1.5) / 2</key>
			<key ordinal="2">sqrt(4)</key>
			<key ordinal="3">cos(45pi / 180)</key>
			<key ordinal="4">1 + 2</key>
			<key ordinal="5">2 + 3</key>
		</inputs>
	</workspace>
	<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
		<inputs>
			<key ordinal="1">4+5*3</key>
			<key ordinal="2">(4+5)*3</key>
			<key ordinal="3">sqrt((4+5)*3)</key>
		</inputs>
	</workspace>
</workspaces>
"""
		);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThat);
		File.Delete(tempFilePathThat);

		// Act
		workspacesvmThis.TEST_Synchronize(workspacesvmThat);

		// Assert
		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		foreach (var workspacevmThis in workspacesvmThis.TheWorkspaceViewModels)
		{
			// Find the corresponding workspace in workspacesThat
			WorkspaceViewModel? workspacevmThat = workspacesvmThat.TheWorkspaceViewModels.FirstOrDefault(wvm => wvm.ID == workspacevmThis.ID);
			Assert.IsNotNull(workspacevmThat);

			// Assert that the History collections are the same
			CollectionAssert.AreEqual(workspacevmThis.History, workspacevmThat.History);
		}
	}

	/// <summary>
	/// Test synchronizing workspace inputs.
	/// </summary>
	[TestMethod]
	public void TestSyncWorkspaceInputsBothModified()
	{
		const string SELECTED_ID = "238b2709-ca68-4643-b4f9-be5be0ea55a2";

		// Arrange
		string tempFilePathThis = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThis,
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
				<key ordinal="4">11 + 22</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
				<key ordinal="4">1 + 2</key>
				<key ordinal="5">2 + 3</key>
				<key ordinal="6">3 + 4</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmThis = WorkspacesViewModel.ConstructFromFile(tempFilePathThis);
		File.Delete(tempFilePathThis);

		string tempFilePathThat = Path.GetTempFileName();
		File.WriteAllText(tempFilePathThat,
"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
				<key ordinal="4">1 + 2</key>
				<key ordinal="5">2 + 3</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
				<key ordinal="4">11 + 22</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmThat = WorkspacesViewModel.ConstructFromFile(tempFilePathThat);
		File.Delete(tempFilePathThat);

		// Act
		workspacesvmThis.TEST_Synchronize(workspacesvmThat);

		// Assert
		// They should be the same after synchronization.
		AssertWorkspacesViewModelEquals(workspacesvmThis, workspacesvmThat);

		// Find new IDs
		string idMainConflict = workspacesvmThis.TheWorkspaceViewModels.Single(w => w.Name == "Main [CONFLICT]").ID;
		string idThatConflict = workspacesvmThis.TheWorkspaceViewModels.Single(w => w.Name == "That [CONFLICT]").ID;

		string tempFilePathTarget = Path.GetTempFileName();
		File.WriteAllText(tempFilePathTarget,
$$"""
<?xml version="1.0" encoding="utf-8"?>
<calculatex>
	<workspaces>
		<workspace id="{{idMainConflict}}" name="Main [CONFLICT]" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
				<key ordinal="4">1 + 2</key>
				<key ordinal="5">2 + 3</key>
			</inputs>
		</workspace>
		<workspace id="238b2709-ca68-4643-b4f9-be5be0ea55a2" name="Main" last-modified="2024-09-06T22:50:31.9513935+00:00" selected="true">
			<inputs>
				<key ordinal="1">34.5 (23 + 1.5) / 2</key>
				<key ordinal="2">sqrt(4)</key>
				<key ordinal="3">cos(45pi / 180)</key>
				<key ordinal="4">11 + 22</key>
			</inputs>
		</workspace>
		<workspace id="{{idThatConflict}}" name="That [CONFLICT]" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
				<key ordinal="4">11 + 22</key>
			</inputs>
		</workspace>
		<workspace id="73668c98-34de-4c87-9bb6-ba33eafd7874" name="That" last-modified="2024-08-24T02:36:31.9513935+00:00" selected="false">
			<inputs>
				<key ordinal="1">4+5*3</key>
				<key ordinal="2">(4+5)*3</key>
				<key ordinal="3">sqrt((4+5)*3)</key>
				<key ordinal="4">1 + 2</key>
				<key ordinal="5">2 + 3</key>
				<key ordinal="6">3 + 4</key>
			</inputs>
		</workspace>
	</workspaces>
</calculatex>
"""
		);
		WorkspacesViewModel workspacesvmTarget = WorkspacesViewModel.ConstructFromFile(tempFilePathTarget);
		//workspacesvmTarget._workspaces.LastSynchronizedUTC = workspacesvmThis.LastSynchronizedUTC;
		File.Delete(tempFilePathTarget);

		// Note: The order of elements is NOT guaranteed, so this might fail. If so, these asserts are invalid.
		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThis);
		AssertWorkspacesViewModelEquals(workspacesvmTarget, workspacesvmThat);

		CollectionAssert.AreEqual(workspacesvmThat.TheWorkspaceViewModels, workspacesvmThis.TheWorkspaceViewModels);

		Assert.AreEqual(workspacesvmTarget.TheWorkspaceViewModels.Count, workspacesvmThis.TheWorkspaceViewModels.Count);
		CollectionAssert.AllItemsAreUnique(workspacesvmTarget.TheWorkspaceViewModels);
		CollectionAssert.AllItemsAreUnique(workspacesvmThis.TheWorkspaceViewModels);

		Assert.AreEqual(0, workspacesvmThis.TheWorkspaceViewModels.Except(workspacesvmThat.TheWorkspaceViewModels).Count());
		Assert.AreEqual(0, workspacesvmThat.TheWorkspaceViewModels.Except(workspacesvmThis.TheWorkspaceViewModels).Count());


		CollectionAssert.AreEquivalent(workspacesvmTarget.TheWorkspaceViewModels, workspacesvmThis.TheWorkspaceViewModels);
		CollectionAssert.AreEquivalent(workspacesvmTarget.TheWorkspaceViewModels, workspacesvmThat.TheWorkspaceViewModels);

		CollectionAssert.AreEqual(workspacesvmTarget.TheWorkspaceViewModels.OrderBy(w => w.ID).ToList(),
											workspacesvmThis.TheWorkspaceViewModels.OrderBy(w => w.ID).ToList());
		CollectionAssert.AreEqual(workspacesvmTarget.TheWorkspaceViewModels.OrderBy(w => w.ID).ToList(),
											workspacesvmThat.TheWorkspaceViewModels.OrderBy(w => w.ID).ToList());
	}

	[TestMethod]
	public void TestWorkspaceNotEquals()
	{
		// Arrange
		Models.Workspace workspace1 = new("cat", name: "fish");
		Models.Workspace workspace2 = new("dog", name: "fish");

		// Act
		bool result = workspace1.Equals(workspace2);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TestWorkspaceEquals()
	{
		// Arrange
		Models.Workspace workspace1 = new("cat", name: "fish");
		Models.Workspace workspace2 = new("cat", name: "dog");

		// Act
		bool result = workspace1.Equals(workspace2);

		// Assert
		Assert.IsTrue(result);
	}

	private static void AssertWorkspacesViewModelEquals(WorkspacesViewModel workspacesvm1, WorkspacesViewModel workspacesvm2)
	{
		// The selected workspace DOES NOT have to be the same.
		//Assert.AreEqual(workspacesvm1.SelectedWorkspaceVM, workspacesvm2.SelectedWorkspaceVM);

		Assert.AreEqual(workspacesvm1.TheWorkspaceViewModels.Count, workspacesvm2.TheWorkspaceViewModels.Count);
		CollectionAssert.AreEqual(workspacesvm1.TEST_Workspaces._deletedWorkspaces, workspacesvm2.TEST_Workspaces._deletedWorkspaces);
		foreach (var workspacevm1 in workspacesvm1.TheWorkspaceViewModels)
		{
			var workspacevm2 = workspacesvm2.TheWorkspaceViewModels.SingleOrDefault(wvm => wvm.ID == workspacevm1.ID);
			Assert.IsNotNull(workspacevm2, $"Workspace with ID {workspacevm1.ID} not found in workspacesvm2.");
			AssertWorkspaceViewModelEquals(workspacevm1, workspacevm2);
		}
	}

	private static void AssertWorkspaceViewModelEquals(WorkspaceViewModel workspacevm1, WorkspaceViewModel workspacevm2)
	{
		Assert.AreEqual(workspacevm1.ID, workspacevm2.ID);
		Assert.AreEqual(workspacevm1.Name, workspacevm2.Name);
		CollectionAssert.AreEqual(workspacevm1.History, workspacevm2.History);
		Assert.AreEqual(workspacevm1.TEST_workspace, workspacevm2.TEST_workspace);
	}
}
