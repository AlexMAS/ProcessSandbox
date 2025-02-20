import sys
print("Line #1")
print("Error Line #1", file=sys.stderr)
print("Line #2")
print("Error Line #2", file=sys.stderr)
print("Line #3")
print("Error Line #3", file=sys.stderr)
exit(1)