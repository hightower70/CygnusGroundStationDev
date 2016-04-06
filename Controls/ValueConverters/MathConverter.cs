	/*
	 * MathConverter and accompanying samples are copyright (c) 2011 by Ivan Krivyakov
	 * ivan [at] ikriv.com
	 * They are distributed under the Apache License http://www.apache.org/licenses/LICENSE-2.0.html
	 */
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Markup;

namespace CygnusControls
{
	/// <summary>
	/// Value converter that performs arithmetic calculations over its argument(s)
	/// </summary>
	/// <remarks>
	/// MathConverter can act as a value converter, or as a multivalue converter (WPF only).
	/// It is also a markup extension (WPF only) which allows to avoid declaring resources,
	/// ConverterParameter must contain an arithmetic expression over converter arguments. Operations supported are +, -, * and /
	/// Single argument of a value converter may referred as x, a, or {0}
	/// Arguments of multi value converter may be referred as x,y,z,t (first-fourth argument), or a,b,c,d, or {0}, {1}, {2}, {3}, {4}, ...
	/// The converter supports arithmetic expressions of arbitrary complexity, including nested subexpressions
	/// </remarks>
	public class MathConverter :
#if !SILVERLIGHT
 MarkupExtension,
			IMultiValueConverter,
#endif
 IValueConverter
	{
		#region · Converter functions ·

		Dictionary<string, IExpression> m_storedExpressions = new Dictionary<string, IExpression>();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new object[] { value }, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				double result = Parse(parameter.ToString()).Eval(values);

				if (double.IsNaN(result))
					result = m_default_value;

				if (targetType == typeof(decimal)) return (decimal)result;
				if (targetType == typeof(string)) return result.ToString();
				if (targetType == typeof(int)) return (int)result;
				if (targetType == typeof(double)) return (double)result;
				if (targetType == typeof(long)) return (long)result;
				throw new ArgumentException(String.Format("Unsupported target type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				ProcessException(ex);
			}

			return DependencyProperty.UnsetValue;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

#if !SILVERLIGHT
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
#endif
		protected virtual void ProcessException(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		private IExpression Parse(string s)
		{
			IExpression result = null;
			if (!m_storedExpressions.TryGetValue(s, out result))
			{
				result = new Parser().Parse(s);
				m_storedExpressions[s] = result;
			}

			return result;
		}

		#endregion

		#region · Singleton instance handling ·
		// No singleton is implemented, because the class caches some data
		// and in the case of singleton memory modell, the cache never cleaned
		#endregion

		#region · Properties ·

		private double m_default_value = 0;

		/// <summary>
		/// Gets/sets default value. This will be the result when the expression gives NaN result.
		/// </summary>
		public double DefaultValue
		{
			get { return m_default_value; }
			set { m_default_value = value; }
		}
		#endregion

		#region · Expression elements ·

		/// <summary>
		/// Expression element (interface)
		/// </summary>
		interface IExpression
		{
			double Eval(object[] args);
		}

		/// <summary>
		/// Constant expression element
		/// </summary>
		class Constant : IExpression
		{
			private double m_value;

			public Constant(string text)
			{
				if (!double.TryParse(text, out m_value))
				{
					throw new ArgumentException(String.Format("'{0}' is not a valid number", text));
				}
			}

			public double Eval(object[] args)
			{
				return m_value;
			}
		}

		/// <summary>
		/// Variable expression element
		/// </summary>
		class Variable : IExpression
		{
			private int _index;

			public Variable(string text)
			{
				if (!int.TryParse(text, out _index) || _index < 0)
				{
					throw new ArgumentException(String.Format("'{0}' is not a valid parameter index", text));
				}
			}

			public Variable(int n)
			{
				_index = n;
			}

			public double Eval(object[] args)
			{
				if (_index >= args.Length)
				{
					throw new ArgumentException(String.Format("MathConverter: parameter index {0} is out of range. {1} parameter(s) supplied", _index, args.Length));
				}

				return System.Convert.ToDouble(args[_index]);
			}
		}

		/// <summary>
		/// Binary operator expression element
		/// </summary>
		class BinaryOperation : IExpression
		{
			private Func<double, double, double> m_operation;
			private IExpression m_left;
			private IExpression m_right;

			public BinaryOperation(char operation, IExpression left, IExpression right)
			{
				m_left = left;
				m_right = right;
				switch (operation)
				{
					case '+': m_operation = (a, b) => (a + b); break;
					case '-': m_operation = (a, b) => (a - b); break;
					case '*': m_operation = (a, b) => (a * b); break;
					case '/': m_operation = (a, b) => (a / b); break;
					default: throw new ArgumentException("Invalid operation " + operation);
				}
			}

			public double Eval(object[] args)
			{
				return m_operation(m_left.Eval(args), m_right.Eval(args));
			}
		}

		/// <summary>
		/// Unary (negate) operator expression element
		/// </summary>
		class Negate : IExpression
		{
			private IExpression m_param;

			public Negate(IExpression in_param)
			{
				m_param = in_param;
			}

			public double Eval(object[] in_args)
			{
				return -m_param.Eval(in_args);
			}
		}
		#endregion

		#region · Expression parser ·

		/// <summary>
		/// Parser class
		/// </summary>
		class Parser
		{
			private string text;
			private int pos;

			public IExpression Parse(string text)
			{
				try
				{
					pos = 0;
					this.text = text;
					var result = ParseExpression();
					RequireEndOfText();
					return result;
				}
				catch (Exception ex)
				{
					string msg =
							String.Format("MathConverter: error parsing expression '{0}'. {1} at position {2}", text, ex.Message, pos);

					throw new ArgumentException(msg, ex);
				}
			}

			private IExpression ParseExpression()
			{
				IExpression left = ParseTerm();

				while (true)
				{
					if (pos >= text.Length) return left;

					var c = text[pos];

					if (c == '+' || c == '-')
					{
						++pos;
						IExpression right = ParseTerm();
						left = new BinaryOperation(c, left, right);
					}
					else
					{
						return left;
					}
				}
			}

			private IExpression ParseTerm()
			{
				IExpression left = ParseFactor();

				while (true)
				{
					if (pos >= text.Length) return left;

					var c = text[pos];

					if (c == '*' || c == '/')
					{
						++pos;
						IExpression right = ParseFactor();
						left = new BinaryOperation(c, left, right);
					}
					else
					{
						return left;
					}
				}
			}

			private IExpression ParseFactor()
			{
				SkipWhiteSpace();
				if (pos >= text.Length) throw new ArgumentException("Unexpected end of text");

				var c = text[pos];

				if (c == '+')
				{
					++pos;
					return ParseFactor();
				}

				if (c == '-')
				{
					++pos;
					return new Negate(ParseFactor());
				}

				if (c == 'x' || c == 'a') return CreateVariable(0);
				if (c == 'y' || c == 'b') return CreateVariable(1);
				if (c == 'z' || c == 'c') return CreateVariable(2);
				if (c == 't' || c == 'd') return CreateVariable(3);

				if (c == '(')
				{
					++pos;
					var expression = ParseExpression();
					SkipWhiteSpace();
					Require(')');
					SkipWhiteSpace();
					return expression;
				}

				if (c == '{')
				{
					++pos;
					var end = text.IndexOf('}', pos);
					if (end < 0) { --pos; throw new ArgumentException("Unmatched '{'"); }
					if (end == pos) { throw new ArgumentException("Missing parameter index after '{'"); }
					var result = new Variable(text.Substring(pos, end - pos).Trim());
					pos = end + 1;
					SkipWhiteSpace();
					return result;
				}

				const string doubleRegEx = @"(\d+\.?\d*|\d*\.?\d+)";
				var match = Regex.Match(text.Substring(pos), doubleRegEx);
				if (match.Success)
				{
					pos += match.Length;
					SkipWhiteSpace();
					return new Constant(match.Value);
				}
				else
				{
					throw new ArgumentException(String.Format("Unexpeted character '{0}'", c));
				}
			}

			private IExpression CreateVariable(int n)
			{
				++pos;
				SkipWhiteSpace();
				return new Variable(n);
			}

			private void SkipWhiteSpace()
			{
				while (pos < text.Length && Char.IsWhiteSpace((text[pos]))) ++pos;
			}

			private void Require(char c)
			{
				if (pos >= text.Length || text[pos] != c)
				{
					throw new ArgumentException("Expected '" + c + "'");
				}

				++pos;
			}

			private void RequireEndOfText()
			{
				if (pos != text.Length)
				{
					throw new ArgumentException("Unexpected character '" + text[pos] + "'");
				}
			}
		}

		#endregion
	}
}