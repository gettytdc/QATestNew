
import java.awt.*;
import java.awt.event.*;
import javax.swing.*;
import javax.swing.event.*;
import javax.swing.tree.*;
import java.net.URL;
import java.beans.*;
import java.util.Random;


public class BPJavaTest extends JPanel implements ActionListener, PropertyChangeListener, ChangeListener, TreeSelectionListener
{

	// Constants
	private static final int HORIZONTAL = 0;
	private static final int VERTICAL = 1;
		
	private JScrollBar _scroll;
	private JCheckBox _chk;
	private JRadioButton _radio0;
	private JRadioButton _radio1;
	private JRadioButton _radio2;
	private JButton _button;
	private JTextField _text;
	private JSlider _slider;
	private SpinnerDemo _spinnerdemo;
	private JSpinner.NumberEditor _numberspinner;
	private JList _datalist;
	private JTree _tree;
	private JToolBar _toolbar;
	private JProgressBar _progressbar;
	private Task _task; // background task run in separate thread so that progress bar has some meaningful data
	private JComboBox _combobox;
	private JComboBox _editablecombobox;
	private JPasswordField _password;
	private JMenuBar _menubar;
	private JTextArea _textarea;
	private JToggleButton _togglebutton;
	private TextSamplerDemo _textsampler;
	
	public static void main(String args[])
	{
	// Create the BP demo
		BPJavaTest instance = new BPJavaTest();
		instance.CreateTestApplet();
		
		//Create dialog demo
		javax.swing.SwingUtilities.invokeLater(new Runnable() {
            public void run()
            {
               JFrame frame = new JFrame("DialogDemo");
				frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
				DialogDemo newContentPane = new DialogDemo(frame);
				newContentPane.setOpaque(true);
				frame.setContentPane(newContentPane);
				frame.pack();
				frame.setVisible(true);
			}
        });
	}
	
	public void CreateTestApplet()
	{
		JFrame mainframe=new JFrame("BPJavaTest");

		JPanel p=new JPanel();

		// Where the GUI is created:
		BuildMenuBar();
		p.add(_menubar);
		mainframe.setJMenuBar(_menubar);
		

		// A toolbar
		_toolbar = new JToolBar("Still draggable");
		_toolbar.add(makeNavigationButton("Back24", "Previous", "Back to previous something-or-other", "Previous"));
		_toolbar.add(makeNavigationButton("Up24", "Up", "Up to something-or-other", "Up"));
        _toolbar.add(makeNavigationButton("Forward24", "Forward", "Forward to something-or-other", "Next"));
		p.add(_toolbar);
		
        // A label...
		JLabel label=new JLabel("Blue Prism Java Test");
		p.add(label);

		// A normal button, which pops up a dialog when clicked
		_button=new JButton("Press Me");
		_button.addActionListener(this);
		_button.setName("Button");
		p.add(_button);

		// A checkbox...
		_chk=new JCheckBox("Check me",false);
		_chk.setName("Checkbox");
		_chk.addChangeListener(this);
		p.add(_chk);

		// A set of radio buttons...
		JPanel rpanel=new JPanel();
		ButtonGroup grp=new ButtonGroup();
		_radio0 =new JRadioButton("Choose me");
		_radio0.setName("Radio 1");
		_radio0.addChangeListener(this);
		_radio1 =new JRadioButton("No, choose me");
		_radio1.setName("Radio 2");
		_radio1.addChangeListener(this);
		_radio2 =new JRadioButton("No, me!");
		_radio2.addChangeListener(this);
		_radio2.setName("Radio 3");
		rpanel.add(_radio0); rpanel.add(_radio1); rpanel.add(_radio2);
		grp.add(_radio0); grp.add(_radio1); grp.add(_radio2);
		p.add(rpanel);

		// An edit field textbox
		_text = new JTextField(40);
		_text.setName("textbox");
		p.add(_text);
		
		// A scrollbar
		_scroll=new JScrollBar(VERTICAL, 0,  20, 0,100); // orientation, value, visibleamount (aka extent), min, max
		_scroll.setName("ScrollBar");
		_scroll.setUnitIncrement(5);
		_scroll.setBlockIncrement(20);
		_scroll.getModel().addChangeListener(this);
		p.add(_scroll);

		// A slider
		_slider = new JSlider(HORIZONTAL, 0, 50, 25);
		_slider.setName("Slider");
		_slider.addChangeListener(this);
		_slider.setMajorTickSpacing(10);
		_slider.setMinorTickSpacing(2);
		_slider.setPaintTicks(true);
		_slider.setPaintLabels(true);
		_slider.addChangeListener(this);
		p.add(_slider);

		// A date spinner
		_spinnerdemo = new SpinnerDemo(true);
		p.add(_spinnerdemo);

		// A list
		String[] data = {"one", "two", "three", "four"};
		_datalist = new JList(data);
		_datalist.setName("List");
		p.add(_datalist);
		
		// A treeview
		DefaultMutableTreeNode TopNode = new DefaultMutableTreeNode("The Java Series");
		createNodes(TopNode);
		_tree = new JTree(TopNode);
		_tree.getSelectionModel().setSelectionMode(TreeSelectionModel.SINGLE_TREE_SELECTION);
		JScrollPane treeView = new JScrollPane(_tree);
		_tree.addTreeSelectionListener(this);
		p.add(_tree);

		// A progress bar
		_progressbar = new JProgressBar(0, 100);
        _progressbar.setValue(0);
        _progressbar.setStringPainted(true);
        p.add(_progressbar);
        _task = new Task();
        _task.addPropertyChangeListener(this);
        _task.execute();
            
        // Two combo boxes
        String[] petStrings = { "Bird", "Cat", "Dog", "Rabbit", "Pig", "Horse", "Mouse", "Hamster", "Canary", "Parrot", "Snake", "Spider" };
        _combobox = new JComboBox(petStrings);
		_combobox.setSelectedIndex(4);
		_combobox.addActionListener(this);
		p.add(_combobox);
		_editablecombobox = new JComboBox(petStrings);
		_editablecombobox.setSelectedIndex(1);
		_editablecombobox.setEditable(true);
		_editablecombobox.addActionListener(this);
		p.add(_editablecombobox);
		
		// Password Field
		_password = new JPasswordField();
		_password.setText("Secret");
		p.add(_password);
				
		// Multiline text area
		_textarea = new JTextArea();
		_textarea.setText("abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r" +
								"abcdefghijklmnopqrstuvwxyz" + "\n\r");
		p.add(_textarea);
		
		// Toggle button
		_togglebutton = new JToggleButton();
		_togglebutton.addChangeListener(this);
		_togglebutton.setText("Toggle Button");
		p.add(_togglebutton);
				
		// A table
		BuildTable(p);
		
		// A tab control
		BuildTabControl(p);
		
		// A text sample
		_textsampler = new TextSamplerDemo();
		p.add(_textsampler);
				
					
		// Put everything into the main frame...
		mainframe.add(p);
		mainframe.pack();

		// We need this handler to exit the application when the window
		// is closed...
        mainframe.addWindowListener(new WindowAdapter()
        	{
				public void windowClosing(WindowEvent e)
				{
                System.exit(0);
            	}
        	});
        
        // Finally, make everything visible...
		mainframe.setVisible(true);

	}
	
	// Handle property change event for various items
	public void propertyChange(PropertyChangeEvent evt)
	{
		Object Source = evt.getSource();
		if (Source == _task)
			{
				if ("progress" == evt.getPropertyName()) 
				{
				int progress = (Integer) evt.getNewValue();
				_progressbar.setValue(progress);
				} 
			}
	}
	
	
	// Handle change state event for various controls
	public void stateChanged(ChangeEvent e)
	{
		String SenderControl = ((Component) e.getSource()).getName();
		String State = "";
		Object Source = e.getSource();
		
		if (Source == _scroll)
		{
			State = "" + _scroll.getValue();
		}
		else if (Source == _chk)
		{
			State = String.valueOf(_chk.isSelected());
		}
		else if (Source == _radio0)
		{
			State = String.valueOf(_chk.isSelected());
		}
		else if (Source == _radio1)
		{
			State = String.valueOf(_chk.isSelected());
		}
		else if (Source == _radio2)
		{
			State = String.valueOf(_chk.isSelected());
		}
		else if (Source == _slider)
		{
			State = "" + _slider.getValue();
		}
		else if (Source == _datalist)
		{
			State = _datalist.getSelectedValue().toString();
		}
		else if (Source == _combobox)
		{
			State = _combobox.getSelectedItem().toString();
		}
		else if (Source == _editablecombobox)
		{
			State = _editablecombobox.getSelectedItem().toString();
		}
		else if (Source == _togglebutton)
		{
			State = String.valueOf(_togglebutton.isSelected());
		}
		
		_text.setText(SenderControl + ": " + State);
	}
	
	// Handle action performed event for various controls
	public void actionPerformed(ActionEvent evt)
	{
		Object Source = evt.getSource();
		if (Source == _text)
		{
				_text.setText("textbox action");
		}
		else if (Source == _button)
		{
			_text.setText("Button " + evt.getActionCommand());
		}
	}


	/** Required by TreeSelectionListener interface. */
	public void valueChanged(TreeSelectionEvent e)
	{
		DefaultMutableTreeNode node = (DefaultMutableTreeNode)
						   _tree.getLastSelectedPathComponent();

		if (node == null)
			return;

		Object nodeInfo = node.getUserObject();
		BookInfo BF = (BookInfo) nodeInfo;
		if (node.isLeaf())
		{
			_text.setText("Leaf node:" + BF.bookName);
		} else
		{
			_text.setText("Node:" + BF.bookName);
		}
	}

	
	private void createNodes(DefaultMutableTreeNode top)
	{
		DefaultMutableTreeNode category = null;
		DefaultMutableTreeNode book = null;

		category = new DefaultMutableTreeNode("Books for Java Programmers");
		top.add(category);

		//original Tutorial
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Tutorial: A Short Course on the Basics",
			"tutorial.html"));
		category.add(book);

		//Tutorial Continued
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Tutorial Continued: The Rest of the JDK",
			"tutorialcont.html"));
		category.add(book);

		//JFC Swing Tutorial
		book = new DefaultMutableTreeNode(new BookInfo
			("The JFC Swing Tutorial: A Guide to Constructing GUIs",
			"swingtutorial.html"));
		category.add(book);

		//Bloch
		book = new DefaultMutableTreeNode(new BookInfo
			("Effective Java Programming Language Guide",
		 "bloch.html"));
		category.add(book);

		//Arnold/Gosling
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Programming Language", "arnold.html"));
		category.add(book);

		//Chan
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Developers Almanac",
			 "chan.html"));
		category.add(book);

		category = new DefaultMutableTreeNode("Books for Java Implementers");
		top.add(category);

		//VM
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Virtual Machine Specification",
			 "vm.html"));
		category.add(book);

		//Language Spec
		book = new DefaultMutableTreeNode(new BookInfo
			("The Java Language Specification",
			 "jls.html"));
		category.add(book);
	}
		
	private class BookInfo
	{
		public String bookName;
		public String bookURL;

		public BookInfo(String book, String filename)
		{
			bookName = book;
			bookURL = filename;
		}

		public String toString() {
			return bookName;
		}
	}


	private JButton makeNavigationButton(String imageName, String actionCommand, String toolTipText, String altText)
	{
		//Look for the image.
		String imgLocation = "images/" + imageName + ".gif";
		URL imageURL = JButton.class.getResource(imgLocation);
	   
		//Create and initialize the button.
		JButton button = new JButton();
		button.setActionCommand(actionCommand);
		button.setName(toolTipText);
		button.setToolTipText(toolTipText);
		button.addActionListener(this);

		if (imageURL != null)
		{                    
			button.setIcon(new ImageIcon(imageURL, altText));
		} 
		else
		{                                    
			button.setText(altText);
			System.err.println("Resource not found: " + imgLocation);
		}

		return button;
	}

	/* Background task running in separate thread for progress bar */
	private class Task extends SwingWorker<Void, Void>
	{
		@Override
		public Void doInBackground() {
			Random random = new Random();
			int progress = 0;
			setProgress(0);
			while (progress < 100) {
				//Sleep for up to one second.
				try {
					Thread.sleep(random.nextInt(1000));
				} catch (InterruptedException ignore) {}
				progress += random.nextInt(10);
				setProgress(Math.min(progress, 100));
			}
			return null;
		}
	}
    
    private void BuildMenuBar()
    {
		JMenu menu, submenu;
		JMenuItem menuItem;
		JRadioButtonMenuItem rbMenuItem;
		JCheckBoxMenuItem cbMenuItem;
	
		// Create the menu bar.
		_menubar = new JMenuBar();
	
		//Build the first menu.
		menu = new JMenu("A Menu");
		menu.setMnemonic(KeyEvent.VK_A);
		menu.getAccessibleContext().setAccessibleDescription(
				"The only menu in this program that has menu items");
		_menubar.add(menu);
	
		//a group of JMenuItems
		menuItem = new JMenuItem("A text-only menu item",
								 KeyEvent.VK_T);
		menuItem.setAccelerator(KeyStroke.getKeyStroke(
				KeyEvent.VK_1, ActionEvent.ALT_MASK));
		menuItem.getAccessibleContext().setAccessibleDescription(
				"This doesn't really do anything");
		menu.add(menuItem);
	
		menuItem = new JMenuItem("Both text and icon",
								 new ImageIcon("images/middle.gif"));
		menuItem.setMnemonic(KeyEvent.VK_B);
		menu.add(menuItem);
	
		menuItem = new JMenuItem(new ImageIcon("images/middle.gif"));
		menuItem.setMnemonic(KeyEvent.VK_D);
		menu.add(menuItem);
		
		//a group of radio button menu items
		menu.addSeparator();
		ButtonGroup group = new ButtonGroup();
		rbMenuItem = new JRadioButtonMenuItem("A radio button menu item");
		rbMenuItem.setSelected(true);
		rbMenuItem.setMnemonic(KeyEvent.VK_R);
		group.add(rbMenuItem);
		menu.add(rbMenuItem);
	
		rbMenuItem = new JRadioButtonMenuItem("Another one");
		rbMenuItem.setMnemonic(KeyEvent.VK_O);
		group.add(rbMenuItem);
		menu.add(rbMenuItem);
	
		//a group of check box menu items
		menu.addSeparator();
		cbMenuItem = new JCheckBoxMenuItem("A check box menu item");
		cbMenuItem.setMnemonic(KeyEvent.VK_C);
		menu.add(cbMenuItem);
	
		cbMenuItem = new JCheckBoxMenuItem("Another one");
		cbMenuItem.setMnemonic(KeyEvent.VK_H);
		menu.add(cbMenuItem);
	
		//a submenu
		menu.addSeparator();
		submenu = new JMenu("A submenu");
		submenu.setMnemonic(KeyEvent.VK_S);
	
		menuItem = new JMenuItem("An item in the submenu");
		menuItem.setAccelerator(KeyStroke.getKeyStroke(
				KeyEvent.VK_2, ActionEvent.ALT_MASK));
		submenu.add(menuItem);
	
		menuItem = new JMenuItem("Another item");
		submenu.add(menuItem);
		menu.add(submenu);
	
		//Build second menu in the menu bar.
		menu = new JMenu("Another Menu");
		menu.setMnemonic(KeyEvent.VK_N);
		menu.getAccessibleContext().setAccessibleDescription(
				"This menu does nothing");
		_menubar.add(menu);

    }
    
    /* Creates a table
    @param p The panel to which the table should be added
    */
    private void BuildTable(JPanel p)
    {
		JPanel LocalPanel = new JPanel(new GridLayout(1,0));
		
        String[] columnNames = {"First Name",
                                "Last Name",
                                "Sport",
                                "# of Years",
                                "Vegetarian"};

        Object[][] data = {
            {"Mary", "Campione",
             "Snowboarding", new Integer(5), new Boolean(false)},
            {"Alison", "Huml",
             "Rowing", new Integer(3), new Boolean(true)},
            {"Kathy", "Walrath",
             "Knitting", new Integer(2), new Boolean(false)},
            {"Sharon", "Zakhour",
             "Speed reading", new Integer(20), new Boolean(true)},
            {"Philip", "Milne",
             "Pool", new Integer(10), new Boolean(false)}
        };

        JTable table = new JTable(data, columnNames);
        table.setPreferredScrollableViewportSize(new Dimension(500, 70));
        table.setFillsViewportHeight(true);

       //Create the scroll pane and add the table to it.
        JScrollPane scrollPane = new JScrollPane(table);

        //Add the scroll pane to this panel.
        LocalPanel.add(scrollPane);
        p.add(LocalPanel);
    
    }
    
    /* Creates a tab control
    @param p The panel to which the tab control should be added
    */
     private void BuildTabControl(JPanel p)
    {
		JPanel LocalPanel = new JPanel(new GridLayout(1, 1));
        
        JTabbedPane tabbedPane = new JTabbedPane();
		ImageIcon icon = createImageIcon("images/middle.gif");
        
        JComponent panel1 = makeTextPanel("Panel #1");
        tabbedPane.addTab("Tab 1", icon, panel1,
                "Does nothing");
        tabbedPane.setMnemonicAt(0, KeyEvent.VK_1);
        
        JComponent panel2 = makeTextPanel("Panel #2");
        tabbedPane.addTab("Tab 2", icon, panel2,
                "Does twice as much nothing");
        tabbedPane.setMnemonicAt(1, KeyEvent.VK_2);
        
        JComponent panel3 = makeTextPanel("Panel #3");
        tabbedPane.addTab("Tab 3", icon, panel3,
                "Still does nothing");
        tabbedPane.setMnemonicAt(2, KeyEvent.VK_3);
        
        JComponent panel4 = makeTextPanel(
                "Panel #4 (has a preferred size of 410 x 50).");
        panel4.setPreferredSize(new Dimension(410, 50));
        tabbedPane.addTab("Tab 4", icon, panel4,
                "Does nothing at all");
        tabbedPane.setMnemonicAt(3, KeyEvent.VK_4);
        
        //The following line enables to use scrolling tabs.
        tabbedPane.setTabLayoutPolicy(JTabbedPane.SCROLL_TAB_LAYOUT);
        
        //Add the tabbed pane to this panel.
        LocalPanel.add(tabbedPane);
        p.add(LocalPanel);
    }
    
    protected JComponent makeTextPanel(String text) {
        JPanel panel = new JPanel(false);
        JLabel filler = new JLabel(text);
        filler.setHorizontalAlignment(JLabel.CENTER);
        panel.setLayout(new GridLayout(1, 1));
        panel.add(filler);
        return panel;
    }
    
    protected static ImageIcon createImageIcon(String path) {
        java.net.URL imgURL = BPJavaTest.class.getResource(path);
        if (imgURL != null) {
            return new ImageIcon(imgURL);
        } else {
            System.err.println("Couldn't find file: " + path);
            return null;
        }
    }
 
        
}