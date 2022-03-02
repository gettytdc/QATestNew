HELPFILE - Helpfile creation Read Me

BluePrism bundles a Windows helpfile "AutomateHelp.chm" with it. It can be manually opened by the user or called directly from BluePrism with the corresponding topic pre-selected.

If a helpfile for the current locale exists, it will be opened. If not the default "AutomateHelp.chm" will be used.
Localised versions will get the name "AutomateHelp_xx-XX.chm" for the release version. (E.g. AutomateHelp_ja-JP for Japanese.)

### Creating a new Locale ###
In order to create a new locale xx-XX: (replace 'xx-XX' with the locale added. E.g. ja-JP for Japanese)
	1. Create source files:
		* Create a new directory named 'xx-XX' in './BluePrism.Automate/Help/l10n/'
		* Copy all contents of './BluePrism.Automate/Help/' (with exception of 'i10n') to the newly created directory.
	2. Add Trigger to build help for new locale:
		* In VisualStudio add a new 'Build Event' (Properties -> Buildevents) in the project 'Setup':
			"%programfiles%\HTML Help Workshop\hhc.exe" "$(SolutionDir)BluePrism.Automate\Help\l10n\xx-XX\AutomateHelp.hhp"

	3. Add the newly created file to the installer:
		* In file 'Components.wxs' (project 'Setup') add a new Component to the ComponentGroup "Statics" following this pattern: 
		      <Component Id="AutomateCHM.xx_XX" Guid="*">
				<File Id="AutomateCHM.xx_XX" Source="$(var.Automate.ProjectDir)Help\l10n\xx-XX\AutomateHelp.chm" Name="AutomateHelp_xx-XX.chm" />
			  </Component>
	4. Rebuild project 'Setup'


### Translating a new locale ###

 * In the sub-directories of the locale, translate all *.htm, *.xsl
 * The files 'Table of Contents.hhc' and 'Index.hhk" contain a TOC. Here the values of the items with name "Name" need translation.
 * The sub-directory 'Graphics' contains screenshots that can be replaced by localised ones.