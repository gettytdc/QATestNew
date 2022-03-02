javac *.java
jar cvfm BPJavaTest.jar manifest.txt *.class
@IF NOT %COMPUTERNAME%==BP0015 pause
