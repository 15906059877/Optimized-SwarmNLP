using System;
using System.Reflection;
using System.CodeDom.Compiler;

namespace Evaluator
{
	/// <summary>
	/// Wraps the results of a programmatically-accessed compilation with the warnings generated by the compiler.
	/// </summary>
	public class AssemblyResults
	{
		Assembly assembly;
		CompilerErrorCollection warnings;

		/// <summary>
		/// Gets the <see cref="Type"/> object that represents the specified type from the compiled assembly. <seealso cref="Assembly.GetType"/>
		/// </summary>
		/// <param name="typeName">
		/// The full name of the type.
		/// </param>
		/// <param name="throwOnError">
		/// true to throw an exception if the type is not found; otherwise, a null reference (Nothing in Visual Basic) is returned.
		/// </param>
		/// <returns>
		/// A <see cref="TypeResults"/> object that wraps the specified class with the warnings generated by the compiler.
		/// </returns>
		public TypeResults GetType(string typeName, bool throwOnError)
		{
			return new TypeResults(assembly.GetType(typeName, throwOnError), this);
		}

		/// <summary>
		/// The final <see cref="Assembly"/> produced by the compiler.
		/// </summary>
		public Assembly Assembly
		{
			get
			{
				return assembly;
			}
		}

		/// <summary>
		/// The collection of warnings generated by the compiler.
		/// </summary>
		public CompilerErrorCollection Warnings
		{
			get
			{
				return warnings;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyResults"/> class.
		/// </summary>
		/// <param name="fullResults">
		/// The results of the programmatically-accessed compilation.
		/// </param>
		protected internal AssemblyResults(CompilerResults fullResults)
		{
			if(fullResults.Errors.HasErrors)
			{
				throw new CompilationException(fullResults.Errors);
			}
			else
			{
				assembly = fullResults.CompiledAssembly;
				warnings = fullResults.Errors;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyResults"/> class.
		/// </summary>
		/// <param name="fullResults">
		/// The results of the programmat