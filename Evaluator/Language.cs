using System;
using System.Text;

namespace Evaluator
{
	/// <summary>
	/// A base class for basic specifications of syntax for individual languages
	/// </summary>
	public abstract class Language
	{
		/// <summary>
		/// Generates a statement to allow treatment of classes within an assembly/namespace as if they were local
		/// </summary>
		/// <param name="assemblyName">
		/// The name of the assembly to be treated as local
		/// </param>
		/// <returns>
		/// A statement that will allow treatment of classes within an assembly/namespace as if they were local
		/// </returns>
		public abstract string useStatement(