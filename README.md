# ProcessSandbox

Предоставляет простой и легковесный способ запуска процессов с заданными лимитами (CPU, RAM, I/O и т.д.).

Инструмент создан для работы в Linux, тем не менее может быть использован в Windows, но с некоторыми ограничениями
функциональности. Например, не будут доступна возможность установки системных лимитов - одного из способов защиты.

## Содержание

* [1. Почему не cgroup/Docker/Kubernetes?](#s-1)
  * [1.1. Более простой и однозначный API](#s-1-1)
  * [1.2. Работа в непривилегированном пользовательском режиме](#s-1-2)
  * [1.3. Возможность установки ограничений на исполнение](#s-1-3)
  * [1.4. Мягкие границы на использование ресурсов](#s-1-4)
  * [1.5. Предоставление статистики исполнения](#s-1-5)
* [2. Особенности при сборе статистики](#s-2)
  * [2.1. Особенности получения `MemoryUsage`](#s-2-1)
  * [2.2. Особенности получения `CpuUsage`](#s-2-2)
  * [2.3. Влияние параллелизма на `CpuUsage`](#s-2-3)
* [3. Особенности реализации](#s-3)
  * [3.1. Стандартные потоки ввода/вывода](#s-3-1)
  * [3.2. Завершение работы процесса](#s-3-2)
  * [3.3. Установка системных лимитов](#s-3-3)
* [4. Сборка проекта](#s-4)
* [5. Интеграционное тестирование](#s-5)
* [F.A.Q.](#faq)

<div id='s-1'/>

## 1. Почему не cgroup/Docker/Kubernetes?

*Использование `ProcessSandbox` не заменяет, а дополняет использование доступных средств лимитирования.*

Основные преимущества `ProcessSandbox`:

* Более простой и однозначный API, чем стандартный `System.Diagnostics.Process`.
* Работа в непривилегированном пользовательском режиме.
* Возможность установки ограничений на исполнение.
* Мягкие границы на использование ресурсов.
* Предоставление статистики исполнения.

<div id='s-1-1'/>

### 1.1. Более простой и однозначный API

Стандартный класс `System.Diagnostics.Process` имеет [множество неочевидных тонкостей в использовании](https://gist.github.com/AlexMAS/276eed492bc989e13dcce7c78b9e179d).
Класс `ProcessSandbox` устраняет этот недостаток, скрывая всевозможные сложности. Ниже приведен пример кода, который запускает
некоторый процесс с ограничениями использования CPU и памяти, выводя статистику исполнения в консоль.

```cs
var processSandbox = new ProcessSandbox(new ProcessSandboxStartInfo
{
    Command = "my-calc",
    Arguments = [ "1", "+", "2" ],
    CpuLimit = TimeSpan.FromSeconds(1),
    MemoryLimit = 1_000_000
});

var exitCode = await processSandbox.Start();

if (exitCode != 0 && !processSandbox.SelfCompletion)
{
    switch (exitCode)
    {
        case (int)SpecialExitCode.CpuLimit:
            Console.WriteLine("CPU limit exceeded!");
            break;
        case (int)SpecialExitCode.MemoryLimit:
            Console.WriteLine("Memory limit exceeded!");
            break;
        // ...
    }
}

Console.WriteLine("Exit Code          : " + exitCode);
Console.WriteLine("Elapsed Time, ms   : " + processSandbox.ElapsedTime.TotalMilliseconds);
Console.WriteLine("CPU Usage, ms      : " + processSandbox.CpuUsage.TotalMilliseconds);
Console.WriteLine("Memory Usage, bytes: " + processSandbox.MemoryUsage);
```

> **INFO**
>
> Если нужен более удобный API для запуска внешнего процесса и не нужно устанавливать никаких ограничений на его исполнение,
> то для этих целей лучше посмотреть на более продвинутые инструменты, например, [CliWrap](https://github.com/Tyrrrz/CliWrap)
> или [simple-exec](https://github.com/adamralph/simple-exec). Основная задача `ProcessSandbox` - это возможность лимитирования
> работы процесса, и в этом смысле он более низкоуровневый инструмент, чем вышеуказанные.

<div id='s-1-2'/>

### 1.2. Работа в непривилегированном пользовательском режиме

`ProcessSandbox` работает в непривилегированном пользовательском режиме и по своим возможностям лимитирования в любом
случае будет уступать системным и проработанным механизмам. Между тем, `ProcessSandbox` позволяет делать оценку поведения
наблюдаемого процесса, устраняя большую часть возможных негативных последствий его работы. Благодаря этому появляется
возможность отказаться от использования более тяжеловесных инструментов, ограничившись лишь настройкой прав доступа к
базовым ресурсам ОС (файловая система, сеть и т.п.).

<div id='s-1-3'/>

### 1.3. Возможность установки ограничений на исполнение

При запуске процесса можно установить следующие ограничения:

* пользователь (запускающий процесс должен иметь root-привилегии);
* использование ресурсов (общее время работы, использование CPU и памяти);
* количество символов выводимых через stdout/stderr;
* количество одновременно запущенных потоков;
* максимальный размер создаваемых файлов;
* количество одновременно открытых файлов;
* запрет порождения дочерних процессов.

При нарушении одного из ограничений запущенный процесс и все его потомки завершаются принудительно.

> **WARN**
>
> Нужно учитывать, что программы на Python, .NET и JVM используют больше ресурсов, чем может показаться на первый взгляд.
> Дело в том, что эти программы запускаются с использованием соответствующей оболочки, которая также использует ресурсы.
> Это нужно учитывать, например, при установке ограничений на количество одновременно запущенных потоков и открытых файлов.
> Помимо этого каждая загружаемая динамическая библиотека, используемая программой прямо или косвенно, также увеличивает
> счетчик одновременно открытых фалов.

<div id='s-1-4'/>

### 1.4. Мягкие границы на использование ресурсов

Если процесс запущен с жесткими ограничениями использования ресурсов, которые может гарантировать cgroup/Docker/Kubernetes,
его поведение оценить крайне трудно или невозможно. Например, в случае, если процесс исчерпал лимит использования CPU,
диспетчер задач перестает выделять ему процессорное время и процесс "повисает". В случае исчерпания лимита использования
памяти процесс, скорей всего, будет завершен принудительно. При этом могут пострадать соседние процессы, работающие
в той же группе или контейнере.

Класс `ProcessSandbox` не устанавливает жесткие ограничения по верхней границе использования ресурсов, но отслеживает
ее превышение, формируя корректный и контролируемый вердикт причины принудительного завершения наблюдаемого процесса.
Между тем, `ProcessSandbox` позволяет ОС делать принудительное завершение процесса, если механизм слежения не успевает
отработать, например, из-за большой загрузки CPU.

<div id='s-1-5'/>

### 1.5. Предоставление статистики исполнения

Во время и после завершения наблюдаемого процесса доступны следующие сведения:

* код завершения;
* общее время работы;
* время использования CPU;
* размер используемой памяти;
* количество символов в stdout/stderr;
* признак превышения лимита на вывод в stdout/stderr;
* признак наличия дочерних процессов;
* признак самостоятельного завершения;
* исключение с указанием причины невозможности запуска или прерывания.


<div id='s-2'/>

## 2. Особенности при сборе статистики

Сбор статистики использования ресурсов производится в отдельном потоке, который периодически получает от ОС статистику
контролируемого процесса. Точность получаемых данных в момент опроса определяется ОС. Например, на практике было выявлено,
что `CpuUsage` отдается с точностью до 10мс, поэтому делать опросы чаще, чем 10мс бессмысленно. Если процесс является
короткоживущим, то есть вероятность, что `CpuUsage` и `MemoryUsage` будут равны `0`, поскольку процесс был завершен до того,
как статистика была запрошена в первый раз. Также следует понимать, что статистика может немного отличаться от того, что
было на момент завершения процесса. Например, `CpuUsage` будет чуть меньше реального, но погрешность не может превышать
период опроса (на данный момент 100мс).

<div id='s-2-1'/>

### 2.1. Особенности получения `MemoryUsage`

Для анализа оперативной памяти, используемой процессом, ОС предоставляет показатели двух видов: `Working Set` и `Private Bytes`.
Данные показатели следует рассматривать как два *пересекающихся* множества.

* Показатель `Working Set` включает только ту память, которую процесс использует *физически* (RAM), на данный момент.
  Показатель не включает память, которую процесс также использует, но которая была выгружена во вторичное хранилище (например,
  на жесткий диск, swap-файл).

* Показатель `Private Bytes` определяет память, которую процесс *запросил* у ОС. Не факт, что вся запрошенная память выделена
  или будет выделена физически (RAM).

Поскольку `ProcessSandbox` используется в первую очередь для контроля лимитов, объем используемой памяти определяется как
*максимум* из двух значений - `Private Bytes` и `Working Set`. Контроль `Private Bytes` позволяет детектировать "намерения"
наблюдаемого процесса и выполнить его выгрузку превентивно, до того, как запрошенный блок памяти будет выделен физически
(`Working Set`). Контроль `Working Set` позволяет отследить всплески использования физической памяти. Подобный комбинированный
подход дает достаточно стабильный результат.

<div id='s-2-2'/>

### 2.2. Особенности получения `CpuUsage`

При получении статистики можно заметить, что значение `CpuUsage` варьируется при запуске одной и той же вычислительной нагрузки.
Подобное поведение можно считать нормой и оно определяется рядом обстоятельств, перечисленных ниже.

Во-первых, так называемый [Clock Drift](https://en.wikipedia.org/wiki/Clock_drift), который характерен для любых часов.
Эффект заключается в том, что часы работают не с постоянной частотой, которая в том числе, может зависеть от нагрузки
на систему. Данный недостаток частично компенсирует NTP, который может ускорять или замедлять часы. Однако сама проблема
никуда не уходит. Также следует учитывать, что NTP сильно зависит от стабильности и скорости работы сетевой инфраструктуры.

Во-вторых, Multi-Core/Multi-Socket серверы могут иметь отдельный таймер на каждое ядро, которые не обязательно синхронизированы
друг с другом (см. [Time on Multi-Core, Multi-Socket Servers](https://steveloughran.blogspot.com/2015/09/time-on-multi-core-multi-socket-servers.html)).
ОС может компенсировать эту рассогласованность и в некоторой степени гарантировать потокам приложения монотонное преставление
времени (Monotonic Clock), даже если они выполнялись на разных ядрах. Тем не менее, существование данной проблемы уже дает
понимание, что замер продолжительности выполнения крайне нетривиальная задача. Вполне возможны случаи, когда последовательные
обращения к Monotonic Clock (например, с помощью `System.nanotime()`) будут приводить к прыжкам во времени из-за того, что
код выполнялся на разных ядрах. С другой стороны, только Monotonic Clock может обеспечить точность до микросекунд и ниже,
иного не дано. Часы типа Time-of-Day благодаря NTP регулярно совершают прыжки во времени, имеют крайне большую погрешность,
поэтому не подходят для подобных целей. В общем случае на уровне прикладного кода выходом может служить механизм привязки
к процессору ([Process Affinity](https://en.wikipedia.org/wiki/Processor_affinity)).

В-третьих, визуализация. У виртуальных машин (VM) аппаратные часы виртуализированы, что создает дополнительные сложности
для приложений, которые должны максимально точно замерять продолжительность выполнения чего-либо. Когда доступ к ядру разделен
между несколькими виртуальными машинами, каждая VM приостанавливается на десятки миллисекунд до тех пор, пока другая VM
работает. С точки зрения приложения все может выглядеть так, что часы внезапно прыгают вперед во времени.

Наконец, из-за особенностей работы диспетчера задач ОС сам по себе процесс получения времени также может быть растянут во
времени и погрешность варьируется в зависимости от количества запущенных процессов.

<div id='s-2-3'/>

### 2.3. Влияние параллелизма на `CpuUsage`

С целью выявления влияния параллелизма на значение `CpuUsage` был проведен ряд экспериментов. Для этого одна и та же вычислительная
нагрузка запускалась последовательно и параллельно. На основании экспериментов были сделаны следующие выводы.

* С увеличением количества параллельно работающих процессов для `CpuUsage` уменьшается процент отклонения от математического
  ожидания, но увеличивается его абсолютное значение относительно последовательного выполнения. Иначе говоря, с ростом
  количества параллельно работающих процессов значения `CpuUsage` будут близки к друг другу, но будут значительно превышать
  эталонное время, полученное при последовательном выполнении.

* Для `CpuUsage` существенное отклонение от времени, полученном при последовательном запуске, начинается, когда количество
  параллельно работающих процессов превышает количество физических ядер. Если количество работающих процессов равно количеству
  физических ядер, отклонение составляет до 30%, а далее сразу от 60%.

* Минимальное общее время выполнения (`ElapsedTime`) группы процессов обеспечивается, когда количество параллельно выполняемых
  процессов равно количеству физических ядер. Дальнейшее увеличение параллелизма не приносит пользы, но и не увеличивает
  данный показатель.

Таким образом, для получения правдоподобной статистики по `CpuUsage` количество параллельно выполняемых процессов не должно
превышать количество физических ядер. Для лучшей утилизации системных ресурсов и получения более-менее правдивых показателей
по `CpuUsage` количество параллельно выполняемых процессов должно быть равно количеству физических ядер.


<div id='s-3'/>

## 3. Особенности реализации

Для запуска процесса необходимо создать экземпляр класса `ProcessSandbox`, передав в его конструктор структуру `ProcessSandboxStartInfo`
с параметрами запуска и настройками ограничений.

<div id='s-3-1'/>

### 3.1. Стандартные потоки ввода/вывода

Экземпляр класса `ProcessSandbox` не изменят стандартные потоки ввода/вывода (stdin/stdout/stderr), а перенаправляет их
контролируемому процессу. По умолчанию используется потоки ввода/вывода запускающего процесса, но это поведение можно изменить.

```cs
public record ProcessSandboxStartInfo
{
    public TextReader StandardInput = Console.In;
    public TextWriter StandardOutput = Console.Out;
    public TextWriter StandardError = Console.Error;
    ...
}
```

Перенаправление стандартных потоков ввода/вывода осуществляется асинхронно, не блокируя основной код запускающего приложения.
Если контролируемый процесс завершается, то и перенаправление ввода/вывода прекращается.

<div id='s-3-2'/>

### 3.2. Завершение работы процесса

При нарушении одного из ограничений запущенный процесс и все его потомки завершаются принудительно. Контроль наличия
дочерних процессов и их принудительное завершение осуществляется вне зависимости от того, как был завершен основной процесс -
самостоятельно (не вышел за установленные лимиты) или принудительно (вышел за лимиты).

Завершение дочерних процессов - это дополнительная защита, так как контролируемый процесс может выйти за рамки допустимого -
намеренно или нет. Самый агрессивный сценарий - это когда процесс порождает сам себя. Другой пример - попытка запуска резидентного
процесса, который должен осуществлять какую-то работу за рамками установленных ограничений.

<div id='s-3-3'/>

### 3.3. Установка системных лимитов

При превышении установленных лимитов решение о принудительном завершении контролируемого процесса может быть сформировано
на уровне экземпляра класса `ProcessSandbox` или на уровне ОС. Назовем их первым и вторым уровнем защиты соответственно.

Экземпляр класса `ProcessSandbox` контролирует ресурсы наблюдаемого процесса, периодически получая от ОС необходимую
статистику (см. выше про особенности сбора статистики). Поскольку запускающий и контролируемый процессы работают на одной
машине, в ряде случаев невозможно гарантировать своевременное срабатывание защитных механизмов. Например, если контролируемый
процесс спровоцировал большую загрузку CPU и таким образом замедлил работу диспетчера задач ОС. В подобных случаях срабатывает
второй уровень защиты - системные лимиты, которые устанавливаются перед запуском контролируемого процесса. ОС тоже контролирует
расход ресурсов и в случае превышения одного из них блокирует доступ к соответствующему ресурсу и/или принудительно завершает
контролируемый процесс.

[Системные лимиты](https://linux.die.net/man/2/setrlimit) конфигурируются следующими свойствами структуры `ProcessSandboxStartInfo`:

* `CpuLimitAddition` - прибавка к лимиту использования CPU (`CpuLimit`) для установки системного лимита (`RLIMIT_CPU`);
* `ThreadCountLimit` - лимит на количество одновременно запущенных потоков (`RLIMIT_NPROC`);
* `FileSizeLimit` - лимит в байтах на максимальный размер создаваемых файлов (`RLIMIT_FSIZE`);
* `OpenFileLimit` - лимит на количество одновременно открытых файлов (`RLIMIT_NOFILE`).

#### `CpuLimitAddition`

Если контролируемый процесс ведет себя крайне агрессивно, расходуя все доступные процессорные ресурсы, это может значительно
замедлить выполнение запускающего процесса вплоть до того, что принудительное завершение такого процесса произойдет либо
слишком поздно, либо не произойдет никогда. В этих случаях устанавливается системный лимит на использование CPU (`RLIMIT_CPU`).
Конечно, желательно, когда контролирующий механизм `ProcessSandbox` справляется самостоятельно, так как в этом случае больше
шансов получить корректную статистику о работе процесса. По этой причине системный лимит на CPU лучше устанавливать чуть
выше передаваемого при запуске `CpuLimit`. Для этих целей используется параметр `CpuLimitAddition`. Если `CpuLimit` определен,
то нижняя граница системного лимита (soft limit) на использование CPU (`RLIMIT_CPU`) определяется, как сумма `CpuLimit`
и `CpuLimitAddition`.

#### `ThreadCountLimit`

Лимит `ThreadCountLimit` применяется для *пользователя*, то есть ко всем процессам и потокам, работающим от имени этого
пользователя. Данную особенность следует учитывать при одновременном запуске нескольких процессов. Иначе говоря, если лимит
рассчитан на запуск одного процесса, то при одновременном запуске нескольких таких процессов их работа может быть нарушена,
что приведет к непредсказуемому результату.

> **WARN**
>
> Экспериментальным путем выявлено, что в Kubernetes значение лимита `ThreadCountLimit` контролируется не на уровне контейнера,
> а на уровне *узла* (node). Таким образом, установка данного лимита оказывает косвенное влияние на все поды (pods),
> работающие на одном и том же узле. По этой причине не следует устанавливать слишком маленькое значение для `ThreadCountLimit`,
> иначе в моменты большой загрузки системы и, соответственно, большом количестве параллельно выполняемых процессов, данный
> лимит может быть исчерпан, что приведет к невозможности запуска новых процессов и дестабилизации работы системы. Общая
> рекомендация - установить приемлемо большое значение, учитывающее общую нагрузку на систему.

#### `FileSizeLimit` и `OpenFileLimit`

Возможные атаки на жесткий диск и файловую систему могут быть предотвращены путем установки `FileSizeLimit` и `OpenFileLimit`.
Попытки превышения данных лимитов приведут либо к принудительному завершению контролируемого процесса, либо к недоступности
соответствующего ресурса.

> **WARN**
>
> Нужно учитывать, что каждая загружаемая динамическая библиотека, используемая программой прямо или косвенно, также
> увеличивает счетчик одновременно открытых фалов.



<div id='s-4'/>

## 4. Сборка проекта

Для сборки нужно установить или запустить Docker, после чего выполнить команду:

```sh
./build.sh
```

Артефакты будут находиться в проектных каталогах `bin/publish/`.


<div id='s-5'/>

## 5. Интеграционное тестирование

Для запуска интеграционных тестов нужно установить или запустить Docker, после чего выполнить следующие команды:

```sh
./build
./integration-tests/run.sh
```

Скрипт [`integration-tests/run.sh`](integration-tests/run.sh) находит все тестовые случаи и выполняет каждый в отдельном
Docker-контейнере.

Для интеграционного тестирования создана консольная утилита [`sandbox`](ProcessSandbox.App/README.md). Она запускает
указанную команду с ограничениями, заданными через переменные окружения. Реализация утилиты основана на `ProcessSandbox`
и может служить примером её использования.

Случаи для интеграционного тестирования размещены в каталоге [`tests`](tests). Каждый тестовый случай описывается парой
файлов: код программы и скрипт, который запускает программу под контролем `sandbox`. Для каждого языка программирования
определен свой подкаталог тестовых случаев. Например, [`tests/sandbox/cpp`](tests/sandbox/cpp) - для C++;
[`tests/sandbox/python`](tests/sandbox/python) - для Python и т.д.

Пример описания тестового случая:
* [`memory-limit.cpp`](tests/sandbox/cpp/memory-limit.cpp) - программа
* [`memory-limit.sh`](tests/sandbox/cpp/memory-limit.sh) - скрипт для запуска

Программа реализует какой-то случай, например, долго выполняющийся код, чтение данных из stdin, вывод результата в stdout и т.п.
Скрипт описывает процесс запуска программы с использованием `sandbox`.

В результате выполнения сценария формируется четыре файла, которые сохраняются в каталог [`integration-tests-out`](integration-tests-out):

* `<test-name>.our` - stdout программы
* `<test-name>.err` - stderr программы
* `<test-name>.stat` - статистика использования ресурсов
* `<test-name>.proc` - список запущенных процессов на момент завершения теста

После выполнения тестов можно запустить проверку корректности работы `sandbox`. Для каждого языка программирования определен
свой класс проверки. Например, [`CppSandboxIntegrationTest.cs`](ProcessSandbox.Tests/Integration/CppSandboxIntegrationTest.cs) -
для тестовых случаев на C++, [`PythonSandboxIntegrationTest.cs`](ProcessSandbox.Tests/Integration/PythonSandboxIntegrationTest.cs) -
для тестовых случаев на Python и т.д. Проверка 



<div id='faq'/>

## F.A.Q.

### Огромные значения `MemoryUsage`

Чаще всего вопрос звучит так: *"Почему такие большие значения использованной памяти? Вы действительно позволили приложению
использовать так много?"* В первую очередь следует ознакомиться с разделом "Особенности при сборе статистики". Физически
приложение не израсходовало указанное количество памяти, но запросило её у ОС, а это значит, что рано или поздно запрошенный
объем может быть выделен. По крайней мере, память будет выделяться до тех пор, пока не будет достигнут установленный лимит.
Однако зачем ждать наступление лимита, если намерения наблюдаемого процесса уже ясны и исход очевиден. К тому же, если дожидаться
 наступления лимита и измерять не запрошенную, физически использованную память, то не будет видна разница между двумя процессами,
 запрашивающими разный объем памяти. Текущий подход как раз позволяет увидеть эту разницу. Например, если в коде приложения
 создается массив на 10Гб, то примерно это значение и будет отражено в показателе `MemoryUsage`.

### Статус `MemoryUsage` для корректной программы

Такое возможно, если ОС завершила процесс принудительно из-за нехватки оперативной памяти. В этом случае процесс завершается
с кодом `137`. Данный код интерпретируется как `MemoryUsage`, так как ситуация говорит именно об этом. Причины, по которым
может возникнуть "дефицит" оперативной памяти, могут быть разными. Самый простой случай - действительно небольшой объем
памяти в системе. Более сложный вариант - загруженность системы. Так или иначе, оперативная память является разделяемым
ресурсом, поэтому параллельно исполняемые процессы косвенным образом могут влиять друг на друга. Если один процесс использует
слишком много памяти, второму её может не хватить, и он будет завершен с кодом `137`. Возможные решения: повторное исполнение;
увеличение памяти системы; уменьшение количества параллельно выполняемых процессов.

### Статус `SpecialExitCode.CpuLimit` при значении `CpuUsage` меньше лимита

Такое возможно, если процесс был завершен принудительно самой ОС в следствии превышения верхней границы системного лимита
на использование CPU (`RLIMIT_CPU`). Из-за принудительного завершения статистика контролируемого процесса становится недоступной,
поэтому значение `CpuUsage` содержит последнее полученное значение, которое обычно чуть меньше системного лимита. Возможные
решения: повторное исполнение; увеличение `CpuLimit`; увеличение `CpuLimitAddition`; уменьшение количества параллельно
выполняемых процессов.

### Срабатывает OOM Killer

Out of Memory Killer (OOM Killer) — это механизм ядра Linux, который освобождает оперативную память при ее исчерпании за
счет принудительного завершения некоторых запущенных процессов. OOM Killer пытается найти и принудительно завершить самый
ресурсоёмкий ("жирный") процесс, который наименее активен в системе и имеет самое короткое время жизни. Алгоритм поиска
процессов можно считать неопределенным (он слишком сложный и может меняться от дистрибутива к дистрибутиву, от версии к
версии, зависит от множества динамически меняющихся параметров).

Возможны несколько причин, которые могут привести к принудительному завершению процесса. В первую очередь нужно убедиться,
что система имеет достаточно разумное количество оперативной памяти. Если с выделенными ресурсами всё в порядке, то далее
следует проанализировать код наблюдаемого процесса (если это возможно). С наибольшей вероятностью именно наблюдаемый процесс
привел к ситуации нехватки оперативной памяти. Причиной может быть неэффективный алгоритм; утечка памяти; в более изощренных
случаях наблюдаемый процесс может создавать один или несколько дочерних, что в совокупности также может привести к нехватке
памяти. Самый агрессивный вариант - наблюдаемый процесс является разновидностью fork-бомбы, то есть процессом, который
клонирует сам себя (в ОС Linux функция `fork()` создает копию вызывающего процесса).

Класс `ProcessSandbox` позволяет установить лимиты, в рамках которых будет работать наблюдаемый процесс. Некоторые лимиты
контролируются программно, на уровне экземпляра `ProcessSandbox`, некоторые делегируются средствам ОС. Программный контроль
сделан для возможности вынесения более точного вердикта на случай, если наблюдаемый процесс был завершен принудительно.
Контроль со стороны ОС является последней инстанцией на случай, если процесс начал вести себя крайне агрессивно (например,
нагрузил очередь задач ОС, не оставляя шансов на программный контроль).

Таким образом, OOM Killer может завершить процесс либо в случае, если в системе действительно недостаточно памяти, либо
если наблюдаемый процесс ведет себя крайне агрессивно, не оставляя шансов `ProcessSandbox` самостоятельно разрешить ситуацию.
Подобные случаи крайне редки, но вполне вероятны.
