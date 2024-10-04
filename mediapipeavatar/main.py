#pipe server
from body import BodyThread
from body_orbbec import BodyThreadOrbbec
import time
import struct
import global_vars
from sys import exit

if global_vars.USE_ORBBEC:
    thread = BodyThreadOrbbec()   
else:
    thread = BodyThread()
thread.start()

i = input()
print("Exiting…")        
global_vars.KILL_THREADS = True
time.sleep(2)
thread.stop()
exit()