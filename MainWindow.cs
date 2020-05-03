using System;
using Gtk;

using System.Xml;
using System.IO;
using System.Text;

public partial class MainWindow: Gtk.Window
{	
	private string [] listOfFiles = new string[10000];		// A list of all of the prayer files in the directory
	private int currentItemIndex;							// The current item that is selected. Index of listOfFiles
	private bool NotFirstTimeRun;								// Determines the first time the main is run.
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{

		Build ();
		GetDirectoryFiles();
		DisplayItem();
		this.NotFirstTimeRun = true;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		ExitApplication();
		a.RetVal = true;
	}

	protected void OnExitButtonClicked (object sender, System.EventArgs e)
	{
		
		checkForChanges();
		ExitApplication();

	}

	protected void ExitApplication() {
		/// If they choose to exit the application, the application quits.
		/// Otherwise, do nothing (return to regular program flow.
		
		if ( YesNo("Exit Open Hands", "Are you sure you want to exit?") == 1)
			Application.Quit ();
	}
	
	protected void OnAddItemClicked (object sender, System.EventArgs e)
	{
		checkForChanges();
		
		string 	newIndex, newIndexFilename, title, entry;
		
		title = "";
		entry = "";	
		
		// Get a new number for a file 
		newIndex = getNewNumber();
		
		// Make the new index into a filename.
		// This is needed to be added to the array.
		// It is just the way that the program evolved. 
		// TODO Work on the listOfFiles array so that it only includes indexes, not filenames.
		newIndexFilename = "./" + newIndex + ".xml";		
		
		// Resize the array to be one longer
		Array.Resize(ref this.listOfFiles, this.listOfFiles.Length + 1);
		
		// Add the current item to the end of the array 
		this.listOfFiles[this.listOfFiles.Length-1] = newIndexFilename;
		
		// Reset the currentItemIndex to reflect the newly entered item.
		currentItemIndex = listOfFiles.Length-1;
		
		
		// Clear the text Areas for a new entry.
		// Assign the title to the title text entry
		ItemTitle.Text = title;
		
		// Assign the text entry the text entry area
		Gtk.TextBuffer ItemTextBuffer;		// Set up a text buffer
		ItemTextBuffer = ItemText.Buffer;	// Create the ItemText buffer.
		ItemTextBuffer.Text =  entry;		// Assign the entry
		
		// When this item is saved, it will:
		// 		- update the directory
		//		- Populate the list again.

	}
	
	public string getNewNumber () 
		/// Generates a new id number.
		/// Used for new items.  Tests to make sure that does not already exist
	{
		string numberString = "";
		int fileNumber = 0;
		
		bool containsNumber = true;

		numberString = extractIndexFromFilename(listOfFiles[listOfFiles.Length-1]); // This is the last item in the list
		//Console.WriteLine(currentItemIndex);
		
		while(containsNumber != false) 
		{
			// Get a random number
			fileNumber = fileNumber + 1;

			// Convert the number to a string
			numberString = fileNumber.ToString();
			
			// Look for the filename in the current array.
			containsNumber = Array.Exists(this.listOfFiles, element => element == ("./" +numberString+ ".xml"));  
	
		}
	
		return numberString;
		
	}
	
	public void WriteXMLFile (string entryID, string titleText, string entryText){
		// Writes the title and entry text to an xml file
		// Filename is the entry ID
		
		string firstLine, xmlOutput, fileName;
		//entryID = "123";
		fileName = entryID + ".xml";
		
		firstLine = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><full>";
		xmlOutput = firstLine + "<title>" + titleText + "</title><entry>" + entryText + "</entry>"+"</full>";
		System.IO.File.WriteAllText(@fileName, xmlOutput);
		
	}
	
	public Tuple<string, string> getXMLEntry (string itemIndex) {
		/// Gets the xml entry information from a file with the name itemIndex
		/// The file information is returned as a Tuple.
	
		// Example from http://msdn.microsoft.com/en-us/library/cc189056%28v=vs.95%29.aspx
		// http://msdn.microsoft.com/en-us/library/aa287537%28v=vs.71%29.aspx
		
		string title, entry, filename;
		
		filename = itemIndex + ".xml";
		
		// Get the xml file
		XmlTextReader reader = new XmlTextReader(filename);
		
		// Get the title text
		while (!reader.EOF && reader.Name != "title")
			reader.Read ();
		title = reader.ReadString();
		

		
		// Get the entry text
		while (!reader.EOF && reader.Name != "entry")
		reader.Read ();
		entry = reader.ReadString();
		
		// Clost the XML Reader
		reader.Close();
		
		return Tuple.Create(title, entry);

	}
	
	public void DisplayItem () {
		
		string title, entry, ItemIDFile, ItemID;
		ItemIDFile = listOfFiles[currentItemIndex];			// Get the filename of the current ID
		ItemID = extractIndexFromFilename(ItemIDFile);		// Get the ItemID from the filename
			

		Tuple <string, string> dataTuple = new Tuple<string, string>("","");
		
		dataTuple = getXMLEntry(ItemID);
		
		title = dataTuple.Item1;
		entry = dataTuple.Item2;
		
		// Assign the title to the title text entry
		ItemTitle.Text = title;
		
		// Assign the text entry the text entry area
		Gtk.TextBuffer ItemTextBuffer;		// Set up a text buffer
		ItemTextBuffer = ItemText.Buffer;	// Create the ItemText buffer.
		ItemTextBuffer.Text =  entry;		// Assign the entry	

	}

	protected void SaveItem() {
		string titleText, entryText, entryID, entryIDFile;

		// Have to have a TextBuffer for the ItemText
		Gtk.TextBuffer ItemTextBuffer;
		
		// Assign the Item text to the buffer.
		ItemTextBuffer = ItemText.Buffer;
		
		// Get the text from the title and the main part.
		titleText = ItemTitle.Text;
		entryText = ItemTextBuffer.Text;
		
		entryIDFile = this.listOfFiles[this.currentItemIndex]; 	// Get the current item text

		entryID = extractIndexFromFilename(entryIDFile);		// Extract just the number
	
		WriteXMLFile(entryID, titleText, entryText);			// Write the file
		GetDirectoryFiles ();  									// Update the list in OH
		
	}
	
	protected void OnUpdateButtonClicked (object sender, System.EventArgs e)
	{

		SaveItem();

	}

	private void GetDirectoryFiles () {
		/// Get a list of all of the files in the directory
		/// Look through these files
		/// Populate the dropdown list
		/// Populate the next/ previous list.
		/// 
		/// The listOfFiles array is only 1000.  
		/// TODO Error catching for lists longer than 1000
		ListStore ClearList = new ListStore(typeof(string));
		ItemChooser.Model = ClearList;
		
		//  Clear the array so that it can be repopulated.
		Array.Clear (this.listOfFiles, 0, this.listOfFiles.Length); 
		
		string itemNumber, title;  // The item number (for use elsewhere)
		
		
		//  Get a list of the xml files in the current directory
		this.listOfFiles = Directory.GetFiles(@"./", "*.xml");
	
		// If there are no files, write one.
		if (this.listOfFiles.Length == 0) {							// If the array lenght == 0
			WriteXMLFile("1", "Sample", "Sample");					// Write the sample file
			this.listOfFiles = Directory.GetFiles(@"./", "*.xml"); 	// Then repopulate the array.
		}
		
		// Set an array for the listbox titles. (As long as the list of files length)
		string [] listboxTitles = new string [this.listOfFiles.Length];
		int listboxTitlesIndex = 0;
		
		// For each of the directory items - put them in the list.
		foreach (string name in this.listOfFiles) {
			
			itemNumber = extractIndexFromFilename(name);
			
			// Get the information from the Item number
			Tuple <string, string> dataTuple = new Tuple<string, string>("","");
			dataTuple = getXMLEntry(itemNumber);
			title = dataTuple.Item1;
			
			// Add the item to the dropdown list
			ItemChooser.AppendText(title);
			ItemChooser.Active = 0;
			
			// Add the item to the array
			listboxTitles[listboxTitlesIndex] = title;
			listboxTitlesIndex++;
			

		}
		
		// Set the current item to the first item.
		this.currentItemIndex = 0;
	}

	protected void OnItemChooserChanged (object sender, System.EventArgs e)
	{

		int comboIndex;
		string name, itemNumber;
		
		comboIndex = ItemChooser.Active;				// Get the current Index from combobox
		
		name = this.listOfFiles[comboIndex];			// Get the filename

		itemNumber = extractIndexFromFilename(name);	// Get the filename from the filename array
		
		this.currentItemIndex = comboIndex;				// Update the universal currentIndexItem
		
		DisplayItem();									// Display the text
		
	}
	
	public string extractIndexFromFilename (string name) 
		/// Takes a filename and strips of the path, and extension
		/// Assumes that the filename is in the current directory "./"
	{
		string  itemNumber;
		int 	extensionIndex; // An index of when the .xml extension starts
		
		extensionIndex = name.IndexOf(".xml") - 2;  // Has to be -2 because of the current directory path
		
		// Get the item number
		itemNumber = name.Substring(2, extensionIndex);
	
		return itemNumber;
	}
	
	public void MessageBox (string msg)
	{	
		MessageDialog md = new MessageDialog (null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, msg);
		md.Run ();
		md.Destroy();
	}

	protected void OnAboutButtonClicked (object sender, System.EventArgs e)
	{
		string aboutString = "   Copyright 2014 - John van Dijk \nCode can be freely used and shared. "
			+ "\n\nTHIS PROGRAM IS PROVIDED 'AS IS' \nWITHOUT WARRANTY OF ANY KIND"; 
		MessageBox(aboutString);
	}

	protected void OnDeleteButtonClicked (object sender, System.EventArgs e)
	{
		
		if ( YesNo("Delete Item", "Are you sure you want to delete this item?") == 1)
		{
			string filenameToDelete;
			
			filenameToDelete = this.listOfFiles[this.currentItemIndex];
			
			File.Delete(filenameToDelete);
			NotFirstTimeRun = false;
			GetDirectoryFiles();
			NotFirstTimeRun = true;
		}
		else
			return;
	}

	protected void OnForwardClicked (object sender, System.EventArgs e)
		/// Increment to the next record, if you are not already at
		/// the last record.
	{
		checkForChanges();

		int filelistLength;
		
		// Find the maximum length of the array.
		filelistLength = this.listOfFiles.Length - 1;
		
		if (this.currentItemIndex == filelistLength)   	// If it is at the last item
		{
			this.currentItemIndex = filelistLength; 	// Nothing changes
		}
		else {
			this.currentItemIndex++;					// Otherwise increment the item
		}
	
		ItemChooser.Active = this.currentItemIndex;		// Set the dropdown to the current index item

		DisplayItem();									// Display the information

	}

	protected void OnBackClicked (object sender, System.EventArgs e)
	{
		checkForChanges();
		
		if (this.currentItemIndex == 0) {
			this.currentItemIndex = 0; }
		else {
			this.currentItemIndex--;
		}
	
		ItemChooser.Active = this.currentItemIndex;
		DisplayItem();
	}
	
	public void printListOfFiles () {
		/// This is a trouble-shooting method
		/// Used to display the array that holds the list of files.		
		
		int i = 0;
		foreach (string name in this.listOfFiles){
			i++;
			Console.WriteLine(name + i.ToString());
		}
		Console.WriteLine(">>>END OF THIS ONE>>>\n");
	}
	
	public int YesNo (string QuestionTitle, string QuestionText) {
		/// An Yes/ No question for any item
		/// http://buttle.shangorilla.com/1.1/handlers/monodoc.ashx?link=T%3AGtk.MessageDialog
		/// 
		int TrueFalse;
		
		MessageDialog md = new MessageDialog 	(null, 
                                      			DialogFlags.DestroyWithParent,
	                              				MessageType.Question, 
                                      			ButtonsType.YesNo, QuestionText);
		md.Title = QuestionTitle;
		
		ResponseType result = (ResponseType)md.Run ();
		
		if (result == ResponseType.Yes)
			TrueFalse = 1;
		else {
			TrueFalse = 0;
		}
		
		md.Destroy();
		return TrueFalse;
	}
	
	private void checkForChanges () {
		
		string SavedEntry, NewEntry, ItemIDFile, ItemID;
		int savedCurrentItemIndex;
		Tuple <string, string> dataTuple = new Tuple<string, string>("","");  // For the item entry later
		
		// Save the current state - we will return to it later.
		savedCurrentItemIndex = this.currentItemIndex;
		
		ItemIDFile = listOfFiles[currentItemIndex];			// Get the filename of the current ID
		ItemID = extractIndexFromFilename(ItemIDFile);		// Get the ItemID from the filename
		
		// Check if the file exists (if it isn't a 'new' item
		if (File.Exists(ItemIDFile)) {
			dataTuple = getXMLEntry(ItemID); // Get the file for that specific item.
		}
		
		// Add the title and the entry together 
		SavedEntry = dataTuple.Item1 + dataTuple.Item2;

		// Have to have a TextBuffer for the ItemText
		Gtk.TextBuffer ItemTextBuffer;
		
		// Assign the Item text to the buffer.
		ItemTextBuffer = ItemText.Buffer;
		
		// Get the text from the title and the main part.
		NewEntry = ItemTitle.Text + ItemTextBuffer.Text;
		
		bool result = SavedEntry.Equals(NewEntry, StringComparison.Ordinal);
		
		// return a value based on the T/F for result.
		// MessageBox(result ? "same" : "not the same");
		Console.WriteLine(ItemIDFile);
		Console.WriteLine(savedCurrentItemIndex);		
		
		// If it is in the index
		if (File.Exists(ItemIDFile)) {
			askForSave(result);
		}
		else
		{
			askForSave(result);
			GetDirectoryFiles();
			savedCurrentItemIndex = 0;
		}
		
		// Reset the current item to the number that it was at the beginning of this function.
		this.currentItemIndex = savedCurrentItemIndex;
		
	}
	
	public void askForSave (bool result) {
		// if the the result is True, ask if there should
		// be a saved entry.  If yes: save the entry.
		
		if (!result) {
			if(YesNo("Save Entry?","Your entry has changed." +
				"\nDo you want to save the entry?") == 1)
				{
					SaveItem();
				}
			
		}
	}
	
	/// TODO Blank out the item (dropdown) number when a new item is added. (until delete, forward, etc. is pressed.)
	/// TODO Add a record number reference at the bottom of the screen ( Record: 1 / 15) so that the user knows where they are in the list.
	/// TODO Change checking for the forward and back button.  (If the entry is changed, have a confirm dialog.)
	/// TODO Set TAB stops. -- so that you can tab through the file IMPORTANT!!!
	/// TODO Hotkeys -- Set hotkeys for each of the items.
	/// 
	/// DONE Post this to sourceforge.
	/// DONE Fix crash when there are no files in the directory. -- [Fixed] -- Now creates a sample file ]
	/// DONE Add resizing ability (Starts at the current size but the user can resize. - All widgets resize, buttons stay in top left corner.)
	/// TODO Fix crash when a new entry and 'check for entry' is called.  It can't find an item to compare it to.  (crashes)
}

