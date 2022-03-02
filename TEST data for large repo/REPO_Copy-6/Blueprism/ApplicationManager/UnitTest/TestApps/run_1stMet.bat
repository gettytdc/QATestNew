cd \blueprism\applicationmanager\unittest\testapps
set classpath=TracePlayer.jar
java com.attachmate.traceplayer.TracePlayer  -VTINIT(NEGOTIATE_0@1stMet.trc_GROOMED) -AUTOMODE(VT) -P(23) -TIMEOUT(-1)
pause
