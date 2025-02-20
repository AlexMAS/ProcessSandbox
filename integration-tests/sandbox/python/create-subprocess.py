import sys
import subprocess
import time

depth = 0
order = 0

if len(sys.argv) >= 2:
    depth = int(sys.argv[1])

if len(sys.argv) >= 3:
    order = int(sys.argv[2])

if order < depth:
    subprocess.run(['python3', __file__, str(depth), str(order + 1)])

time.sleep(1)
