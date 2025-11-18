global using Microsoft.VisualStudio.TestTools.UnitTesting;

// Explicitly disable parallelization for integration tests
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]