#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <sys/types.h>

int main(void)
{
    printf("Real UID = %d\n", getuid());
    printf("Real GID = %d\n", getgid());
    printf("Effective UID = %d\n", geteuid());
    printf("Effective GID = %d\n", getegid());
    return EXIT_SUCCESS;
}
