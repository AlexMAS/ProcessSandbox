def infMult(x):
    return (x * infMult(x+1))
infMult(1)
exit(0)