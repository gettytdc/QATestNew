cd %~dp0

cd C:\Program*\Java\jre*\bin

set PATH=%PATH%;%CD%

cd %~dp0

set classpath=TracePlayer.jar
java com.attachmate.traceplayer.TracePlayer  -VTINIT(NEGOTIATE_0@1stMet.trc_GROOMED) -AUTOMODE(VT) -P(23) -TIMEOUT(-1)

