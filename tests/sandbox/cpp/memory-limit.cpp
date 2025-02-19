#include <unistd.h>

int main(int argc, char** argv)
{
    char *data = new char[600 * 1024 * 1024](); // 600Mb
    sleep(1);
    delete data;
    return 0;
}
