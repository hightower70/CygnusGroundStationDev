///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2015 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Settings parser class
// This file is based on WildcardMatcher Class by Zoran Horvat
// Copyright (c) 2012 www.sysexpand.com
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClassLibrary.Helpers
{
	/// <summary>
	/// Implements string matching algorithm which tries to match pattern containing wildcard characters
	/// with the given input string.
	/// </summary>
	class WildcardStringComparer
	{
		#region · Constants ·

		/// <summary>
		/// Default wildcard character used to map single character from input string.
		/// </summary>
		public const char DefaultSingleWildcard = '?';

		/// <summary>
		/// Default wildcard character used to map zero or more consecutive characters from input string.
		/// </summary>
		public const char DefaultMultipleWildcard = '*';

		#endregion

 		#region · Data members ·
		/// <summary>
		/// Pattern against which input strings are matched; may contain wildcard characters.
		/// </summary>
		private string m_pattern;

		/// <summary>
		/// Wildcard character used to match exactly one character in the input string.
		/// By default this value is question mark (?).
		/// </summary>
		private char m_single_wildcard;

		/// <summary>
		/// Wildcard character used to match zero or more consecutive characters in the input string.
		/// By default this value is asterisk (*).
		/// </summary>
		private char m_multiple_wildcard;

		#endregion

		#region · Constructors ·

		/// <summary>
		/// Default constructor.
		/// </summary>
		public WildcardStringComparer()
			: this(null, DefaultSingleWildcard, DefaultMultipleWildcard)
		{
		}

		/// <summary>
		/// Constructor which initializes pattern against which input strings are matched.
		/// </summary>
		/// <param name="pattern">Pattern used to match input strings.</param>
		public WildcardStringComparer(string pattern)
			: this(pattern, DefaultSingleWildcard, DefaultMultipleWildcard)
		{
		}

		/// <summary>
		/// Constructor which initializes pattern against which input strings are matched and
		/// wildcard characters used in string matching.
		/// </summary>
		/// <param name="pattern">Pattern against which input strings are matched.</param>
		/// <param name="singleWildcard">Wildcard character used to replace single character in input strings.</param>
		/// <param name="multipleWildcard">Wildcard character used to replace zero or more consecutive characters in input strings.</param>
		public WildcardStringComparer(string pattern, char singleWildcard, char multipleWildcard)
		{
			m_pattern = pattern;
			m_single_wildcard = singleWildcard;
			m_multiple_wildcard = multipleWildcard;
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets or sets pattern against which input strings are mapped.
		/// Pattern may contain wildcard characters specified by <see cref="SingleWildcard"/>
		/// and <see cref="MultipleWildcard"/> properties.
		/// Returns empty string if pattern has not been set or it was set to null value.
		/// </summary>
		public string Pattern
		{
			get
			{
				return m_pattern ?? string.Empty;
			}
			set
			{
				m_pattern = value;
			}
		}

		/// <summary>
		/// Gets or sets wildcard character which is used to replace exactly one character
		/// in the input string (default is question mark - ?).
		/// </summary>
		public char SingleWildcard
		{
			get
			{
				return m_single_wildcard;
			}
			set
			{
				m_single_wildcard = value;
			}
		}

		/// <summary>
		/// Gets or sets wildcard character which is used to replace zero or more characters
		/// in the input string (default is asterisk - *).
		/// </summary>
		public char MultipleWildcard
		{
			get
			{
				return m_multiple_wildcard;
			}
			set
			{
				m_multiple_wildcard = value;
			}
		}


		#endregion

		#region · Public functions ·

		/// <summary>
		/// Tries to match <paramref name="value"/> against <see cref="Pattern"/> value stored in this instance.
		/// </summary>
		/// <param name="value">String which should be matched against the contained pattern.</param>
		/// <returns>true if <paramref name="value"/> can be matched with <see cref="Pattern"/>; otherwise false.</returns>
		public bool IsMatch(string value)
		{

			int[] inputPosStack = new int[(value.Length + 1) * (Pattern.Length + 1)];   // Stack containing input positions that should be tested for further matching
			int[] patternPosStack = new int[inputPosStack.Length];                      // Stack containing pattern positions that should be tested for further matching
			int stackPos = -1;                                                          // Points to last occupied entry in stack; -1 indicates that stack is empty
			bool[,] pointTested = new bool[value.Length + 1, Pattern.Length + 1];       // Each true value indicates that input position vs. pattern position has been tested

			int inputPos = 0;   // Position in input matched up to the first multiple wildcard in pattern
			int patternPos = 0; // Position in pattern matched up to the first multiple wildcard in pattern

			if (m_pattern == null)
				m_pattern = string.Empty;

			// Match beginning of the string until first multiple wildcard in pattern
			while (inputPos < value.Length && patternPos < Pattern.Length &&
						 Pattern[patternPos] != MultipleWildcard &&
						 (value[inputPos] == Pattern[patternPos] || Pattern[patternPos] == SingleWildcard))
			{
				inputPos++;
				patternPos++;
			}

			// Push this position to stack if it points to end of pattern or to a general wildcard character
			if (patternPos == m_pattern.Length || m_pattern[patternPos] == m_multiple_wildcard)
			{
				pointTested[inputPos, patternPos] = true;
				inputPosStack[++stackPos] = inputPos;
				patternPosStack[stackPos] = patternPos;
			}

			bool matched = false;

			// Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
			while (stackPos >= 0 && !matched)
			{

				inputPos = inputPosStack[stackPos];         // Pop input and pattern positions from stack
				patternPos = patternPosStack[stackPos--];   // Matching will succeed if rest of the input string matches rest of the pattern

				if (inputPos == value.Length && patternPos == Pattern.Length)
					matched = true;     // Reached end of both pattern and input string, hence matching is successful
				else if (patternPos == Pattern.Length - 1)
					matched = true;     // Current pattern character is multiple wildcard and it will match all the remaining characters in the input string
				else
				{
					// First character in next pattern block is guaranteed to be multiple wildcard
					// So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern

					for (int curInputStart = inputPos; curInputStart < value.Length; curInputStart++)
					{

						int curInputPos = curInputStart;
						int curPatternPos = patternPos + 1;

						while (curInputPos < value.Length && curPatternPos < Pattern.Length &&
									 Pattern[curPatternPos] != MultipleWildcard &&
									 (value[curInputPos] == Pattern[curPatternPos] || Pattern[curPatternPos] == SingleWildcard))
						{
							curInputPos++;
							curPatternPos++;
						}

						// If we have reached next multiple wildcard character in pattern without breaking the matching sequence,
						// then we have another candidate for full match.
						// This candidate should be pushed to stack for further processing.
						// At the same time, pair (input position, pattern position) will be marked as tested,
						// so that it will not be pushed to stack later again.
						if (((curPatternPos == Pattern.Length && curInputPos == value.Length) ||
								 (curPatternPos < Pattern.Length && Pattern[curPatternPos] == MultipleWildcard)) &&
								!pointTested[curInputPos, curPatternPos])
						{
							pointTested[curInputPos, curPatternPos] = true;
							inputPosStack[++stackPos] = curInputPos;
							patternPosStack[stackPos] = curPatternPos;
						}

					}
				}

			}

			return matched;

		}

		#endregion
	}
}
