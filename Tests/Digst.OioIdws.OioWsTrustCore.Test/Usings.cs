global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using FluentAssertions;
global using Moq;

// Explicitly disable parallelization for integration tests
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]