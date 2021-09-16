javac *.java
jar cvfm CGJavaTest.jar manifest.txt *.class
@IF NOT %COMPUTERNAME%==BP0015 pause
