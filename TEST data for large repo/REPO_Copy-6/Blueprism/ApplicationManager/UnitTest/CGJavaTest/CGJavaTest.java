
import java.awt.*;
import java.awt.event.*;
import javax.swing.*;

public class CGJavaTest implements ActionListener
{

	JFrame mainframe;
	JButton button;

	public void init()
	{
		mainframe=new JFrame("CGJavaTest");

		JPanel p=new JPanel();

		// A label...
		JLabel label=new JLabel("Blue Prism Java Test");
		p.add(label);

		// A normal button...
		button=new JButton("Press Me");
		button.setName("A button");
		button.addActionListener(this);
		p.add(button);

		// A checkbox...
		JCheckBox chk=new JCheckBox("Check me",false);
		p.add(chk);

		// A set of radio buttons...
		JPanel rpanel=new JPanel();
		ButtonGroup grp=new ButtonGroup();
		JRadioButton r0=new JRadioButton("Choose me");
		JRadioButton r1=new JRadioButton("No, choose me");
		JRadioButton r2=new JRadioButton("No, me!");
		rpanel.add(r0); rpanel.add(r1); rpanel.add(r2);
		grp.add(r0); grp.add(r1); grp.add(r2);
		p.add(rpanel);

		JTextField textfield=new JTextField("Edit Test");
		p.add(textfield);

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

	public void actionPerformed(ActionEvent event)
	{
		JOptionPane.showMessageDialog(mainframe,"You pressed the button");
	}

	public static void main(String args[])
	{
		CGJavaTest c=new CGJavaTest();
		c.init();
	}

}
