README

This html page is designed to test the launching of processes from a web browser, by
communicating directly with a local resource pc.



INSTRUCTIONS

1) Run automate with the command line parameters /local /resourcepc /user <username> <password>.

2) Load the html page in Internet Explorer. Note that it may not work in other browsers such as
mozilla firefox, because the page makes cross-site http requests, which are disallowed in firefox by default.

3) Select one or more of the processes in the list, and click the "start" button.

4) Debugging information should appear in the free-form text area, and the live status of each session
should be shown in the grey status area, with half second updates.