#include <unistd.h>
#include <stdio.h>

int main(void)
{
    pid_t pid = getpid();
    pid_t pgid = getpgid(pid);

    if (fork() != 0)
    {
        printf("Parent PID = %d, PGID=%d\n", pid, pgid);
        printf("Parent completed!\n");
    }
    else
    {
        // Ребенок продолжает жить без родителя
        printf("Child PID = %d, PGID=%d\n", pid, pgid);
        sleep(10);
        printf("Child completed!\n");
    }

    return 0;
}
