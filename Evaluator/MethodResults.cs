using System;
using System.Reflection;
using System.Reflection.Emit;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

namespace Evaluator
{
	/// <summary>
	/// Wraps the results of a programmatically-accessed compilation of a method with the warnings generated by the compiler.
	/// </summary>
	public class MethodResults
	{
		MethodInfo method;
		TypeResults typeResults;
        FastInvokeHandler FastInvoke;
        object Instance;

		/// <summary>
		/// Invokes the compiled method.
		/// </summary>
		/// <param name="parameters">
		/// The parameters to pass to the method.
		/// </param>
		/// <returns>
		/// The return value of the method.
		/// </returns>
		public object Invoke(params object[] parameters)
		{
			//return method.Invoke(typeResults.Instantiate(), parameters);
            return FastInvoke.Invoke(Instance, parameters);
		}

		/// <summary>
		/// The collection of warnings generated by the compiler.
		/// </summary>
		public CompilerErrorCollection Warnings
		{
			get
			{
				return typeResults.Warnings;
			}
		}

		/// <summary>
		/// The reflected