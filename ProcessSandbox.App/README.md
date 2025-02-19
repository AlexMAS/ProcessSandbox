# sandbox

The `sandbox` provides an ability to run a command with given limits.

## Synopsis

```sh
sandbox RESULT_FILE WORKING_DIR COMMAND [ARGS]
```

For more details run:

```sh
sandbox
```

## Example

The next example runs a process with CPU and memory limits, and prints the execution statistics afterwards.

```sh
SANDBOX_CPU_LIMIT=1000
SANDBOX_MEMORY_LIMIT=1000000
sandbox ./my-calc.stat ./ ./my-calc 1 + 2
cat ./my-calc.stat
```
