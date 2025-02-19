a=int(input())
v=0
for i in range(0,a):
    b=float(input())
    if b/2*10%10 == 5:
        b=(b+1)
        if b/2>v:
            v=b/2
    print(int(v))
