
using System;
using System.Text;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Specialized;

namespace Evaluator
{
	/// <summary>
	/// A class providing static methods for the language-independent runtime compilation of .NET source code.
	/// </summary>
	public class Eval
	{
		/// <summary>
		/// Compiles an assembly from the provided source with the parameters specified.
		/// </summary>
		/// <param name="compiler">
		/// The compiler to use for compiling the source to MSIL.
		/// </param>
		/// <param name="assemblySource">
		/// The actual source of the assembly.
		/// </param>
		/// <param name="options">
		/// The parameters to be set for the compiler.
		/// </param>
		/// <param name="language">
		/// A specification of the syntax of the language of the code
		/// </param>
		/// <returns>
		/// The resulting assembly and any warnings produced by the compiler, wrapped in an AssemblyResults object.
		/// </returns>
		/// <exception cref="CompilationException"/>
		public static AssemblyResults CreateAssembly(ICodeCompiler compiler, string assemblySource, CompilerParameters options, Language language)
		{
			CompilerResults results = compiler.CompileAssemblyFromSource(options, assemblySource);
			return new AssemblyResults(results, assemblySource);
		}

		/// <summary>
		/// Compiles a type (or class) from the provided source with the parameters specified.
		/// </summary>
		/// <param name="compiler">
		/// The compiler to use for compiling the source to MSIL.