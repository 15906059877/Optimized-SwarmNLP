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
		public abstract string useStatement(string assemblyName);

		/// <summary>
		/// Generates a statement to begin a namespace
		/// </summary>
		/// <param name="namespaceName">
		/// The name of the namespace
		/// </param>
		/// <returns>
		/// A statement to begin the namespace
		/// </returns>
		public abstract string beginNamespace(string namespaceName);
		/// <summary>
		/// Generates a statement to end a namespace
		/// </summary>
		/// <param name="namespaceName">
		/// The name of the namespace
		/// </param>
		/// <returns>
		/// A statement to end the namespace
		/// </returns>
		public abstract string endNamespace(string namespaceName);

		/// <summary>
		/// Generates a statement to begin a type
		/// </summary>
		/// <param name="typeName">
		/// The name of the type
		/// </param>
		/// <returns>
		/// A statement to begin the type
		/// </returns>
		public abstract string beginType(string typeName);
		/// <summary>
		/// Generates a statement to end a type
		/// </summary>
		/// <param name="typeName">
		/// The name of the type
		/// </param>
		/// <returns>
		/// A statement to end the type
		/// </returns>
		public abstract string endType(string typeName);
	}

	/// <summary>
	/// A basic specification of the syntax of C#
	/// </summary>
	public class CSharpLanguage : Language
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
		public override string useStatement(string assemblyName)
		{
			return String.Format("using {0};{1}", assemblyName, Environment.NewLine)