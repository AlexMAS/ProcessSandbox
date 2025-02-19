#include <unistd.h>
#include <iostream>
#include <sys/time.h>
#include <sys/resource.h>

using namespace std;

const int MIN_ARG_COUNT = 6;
const int EXIT_CODE_WRONG_ARGS = 198;
const int EXIT_CODE_CANNOT_START_PROCESS = 199;

void printHelp()
{
    cout << "NAME" << endl;
    cout << "    sandbox-exec - run a command with given system limits" << endl;
    cout << "" << endl;
    cout << "SYNOPSIS" << endl;
    cout << "    sandbox-exec RLIMIT_CPU RLIMIT_NPROC RLIMIT_FSIZE RLIMIT_NOFILE COMMAND [ARGS]" << endl;
    cout << "" << endl;
    cout << "DESCRIPTION" << endl;
    cout << "    Start COMMAND, and kill it if given limits are exceeded." << endl;
    cout << "" << endl;
    cout << "    Arguments:" << endl;
    cout << "        RLIMIT_CPU    is the maximum CPU usage time in seconds (-1 if unlimited);" << endl;
    cout << "        RLIMIT_NPROC  is the maximum number of executing threads (-1 if unlimited);" << endl;
    cout << "        RLIMIT_FSIZE  is the maximum file size in bytes that the process may create (-1 if unlimited);" << endl;
    cout << "        RLIMIT_NOFILE is the maximum number of file descriptors that the process may hold open (-1 if unlimited);" << endl;
    cout << "        COMMAND       is the command to execute;" << endl;
    cout << "        ARGS          are the command arguments." << endl;
    cout << "" << endl;
    cout << "    Exit code:" << endl;
    cout << "        198           wrong number of arguments or their format (this help)" << endl;
    cout << "        199           unable to start COMMAND (see errno in stderr)" << endl;
    cout << "        -             the exit code of COMMAND otherwise" << endl;
}

void trySetLimit(int limitType, long softLimit, long hardLimit)
{
    if (softLimit < 0 || softLimit >= RLIM_INFINITY)
    {
        return;
    }

    struct rlimit limit;
    limit.rlim_cur = softLimit;
    limit.rlim_max = hardLimit;

    setrlimit(limitType, &limit);
}

/**
 * Устанавливает системный лимит на использование CPU.
 *
 * Лимит <c>RLIMIT_CPU</c> определяет для процесса максимальное время использования CPU, в секундах.
 * Когда процесс достигает нижней границы лимита (soft limit), система начинает раз в секунду
 * отправлять сигнал <c>SIGXCPU</c>. Когда процесс достигает верхней границы лимита (hard limit),
 * система отправляет сигнал <c>SIGKILL</c>, принудительно завершая работу процесса. Установленный
 * лимит применяется к текущему процессу и всем его потомкам.
 */
void trySetCpuLimit(long value)
{
    trySetLimit(RLIMIT_CPU, value, value + 1);
}

/**
 * Устанавливает системный лимит на количество одновременно запущенных потоков.
 *
 * Лимит <c>RLIMIT_NPROC</c> определяет для <b>пользователя</b> максимальное количество
 * одновременно запущенных потоков. Попытка превзойти этот лимит окончится неудачей,
 * то есть процесс будет неспособен запустить процесс или поток. Установленный лимит
 * применяется к текущему процессу и всем его потомкам.
 */
void trySetThreadCountLimit(long value)
{
    trySetLimit(RLIMIT_NPROC, value, value);
}

/**
 * Устанавливает системный лимит в байтах на максимальный размер создаваемых файлов.
 *
 * Лимит <c>RLIMIT_FSIZE</c> определяет для процесса максимальный размер создаваемых файлов, в байтах.
 * Когда процесс пытается превысить данный лимит, система отправляет ему сигнал <c>SIGXFSZ</c>,
 * который по умолчанию завершает работу процесса. Если процесс обрабатывает и игнорирует данный
 * сигнал, дальнейшие попытки записи в файл не приведут к успеху. Установленный лимит применяется
 * к текущему процессу и всем его потомкам.
 */
void trySetFileSizeLimit(long value)
{
    trySetLimit(RLIMIT_FSIZE, value, value);
}

/**
 * Устанавливает системный лимит на количество одновременно открытых файлов.
 *
 * Лимит <c>RLIMIT_NOFILE</c> определяет для процесса максимальное количество одновременно открытых файлов.
 * Попытка превзойти этот лимит окончится неудачей, то есть процесс будет неспособен открыть новый файл.
 * Установленный лимит применяется к текущему процессу и всем его потомкам.
 */
void trySetOpenFileLimit(long value)
{
    trySetLimit(RLIMIT_NOFILE, value, value);
}

int main(int argc, char** argv)
{
    if (argc < MIN_ARG_COUNT)
    {
        printHelp();
        return EXIT_CODE_WRONG_ARGS;
    }

    // Текущий процесс - лидер группы
    setpgid(0, 0);

    long cpuLimit = atol(*++argv);
    long threadCountLimit = atol(*++argv);
    long fileSizeLimit = atol(*++argv);
    long openFileLimit = atol(*++argv);
    char* command = *++argv;

    int argumentCount = argc - MIN_ARG_COUNT;
    char* argumentList[argc + 2];

    // По соглашению первый параметр - имя команды
    argumentList[0] = command;
    // Последний параметр - NULL
    argumentList[argumentCount + 1] = NULL;

    for (int i = 1; i <= argumentCount; ++i)
    {
        argumentList[i] = argv[i];
    }

    // Установка системных лимитов для текущего процесса и всех его дочерних
    trySetFileSizeLimit(fileSizeLimit);
    trySetOpenFileLimit(openFileLimit);
    trySetThreadCountLimit(threadCountLimit);
    trySetCpuLimit(cpuLimit);

    // Передача управления в точку входа запускаемой команды
    int result = execvp(command, argumentList);

    if (result == -1)
    {
        perror("exec");
        return EXIT_CODE_CANNOT_START_PROCESS;
    }

    return 0;
}
