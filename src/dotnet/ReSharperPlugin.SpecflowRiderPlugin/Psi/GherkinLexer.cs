using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinLexer : ILexer
    {
        private int _currentPosition;
        private int myCurrentTokenStart;
        private int myState;
        private int myEndOffset;
        private string myCurLanguage;
        
        private IReadOnlyCollection<string> myKeywords;
        private readonly GherkinKeywordProvider myKeywordProvider;

        // ReSharper disable InconsistentNaming
        private const int STATE_DEFAULT = 0;
        private const int STATE_AFTER_KEYWORD = 1;
        private const int STATE_TABLE = 2;
        private const int STATE_AFTER_KEYWORD_WITH_PARAMETER = 3;
        private const int STATE_INSIDE_PYSTRING = 5;

        private const int STATE_PARAMETER_INSIDE_PYSTRING = 6;
        private const int STATE_PARAMETER_INSIDE_STEP = 7;

        private const string PYSTRING_MARKER = "\"\"\"";
        // ReSharper restore InconsistentNaming

        public GherkinLexer(IBuffer buffer, GherkinKeywordProvider provider)
        {
            Buffer = buffer;
            myKeywordProvider = provider;
            UpdateLanguage("en");
        }

        public void Start()
        {
            myEndOffset = Buffer.Length;
            _currentPosition = 0;
            myState = 0;
            Advance();
        }
        
        private void UpdateLanguage(string language) {
            myCurLanguage = language;
            myKeywords = myKeywordProvider.GetAllKeywords(language);
        }

        public void Advance()
        {
            if (_currentPosition >= myEndOffset)
            {
                TokenType = null;
                return;
            }

            myCurrentTokenStart = _currentPosition;
            char c = Buffer[_currentPosition];
            if (myState != STATE_INSIDE_PYSTRING && char.IsWhiteSpace(c))
            {
                AdvanceOverWhitespace();
                TokenType = GherkinTokenTypes.WHITE_SPACE;
                while (_currentPosition < myEndOffset && char.IsWhiteSpace(Buffer[_currentPosition]))
                {
                    AdvanceOverWhitespace();
                }
            }
            else if (c == '|' && myState != STATE_INSIDE_PYSTRING)
            {
                TokenType = GherkinTokenTypes.PIPE;
                _currentPosition++;
                myState = STATE_TABLE;
            }
            else if (myState == STATE_PARAMETER_INSIDE_PYSTRING)
            {
                if (c == '>')
                {
                    myState = STATE_INSIDE_PYSTRING;
                    _currentPosition++;
                    TokenType = GherkinTokenTypes.STEP_PARAMETER_BRACE;
                }
                else
                {
                    AdvanceToParameterEnd(PYSTRING_MARKER);
                    TokenType = GherkinTokenTypes.STEP_PARAMETER_TEXT;
                }
            }
            else if (myState == STATE_INSIDE_PYSTRING)
            {
                if (IsStringAtPosition(PYSTRING_MARKER))
                {
                    _currentPosition += 3 /* marker length */;
                    TokenType = GherkinTokenTypes.PYSTRING;
                    myState = STATE_DEFAULT;
                }
                else
                {
                    if (Buffer[_currentPosition] == '<')
                    {
                        if (IsStepParameter(PYSTRING_MARKER))
                        {
                            _currentPosition++;
                            myState = STATE_PARAMETER_INSIDE_PYSTRING;
                            TokenType = GherkinTokenTypes.STEP_PARAMETER_BRACE;
                        }
                        else
                        {
                            _currentPosition++;
                            AdvanceToParameterOrSymbol(PYSTRING_MARKER, STATE_INSIDE_PYSTRING, false);
                            TokenType = GherkinTokenTypes.PYSTRING_TEXT;
                        }
                    }
                    else
                    {
                        AdvanceToParameterOrSymbol(PYSTRING_MARKER, STATE_INSIDE_PYSTRING, false);
                        TokenType = GherkinTokenTypes.PYSTRING_TEXT;
                    }
                }
            }
            else if (myState == STATE_TABLE)
            {
                TokenType = GherkinTokenTypes.TABLE_CELL;
                while (_currentPosition < myEndOffset)
                {
                    // Cucumber: 0.7.3 Table cells can now contain escaped bars - \| and escaped backslashes - \\
                    if (Buffer[_currentPosition] == '\\')
                    {
                        int nextPos = _currentPosition + 1;
                        if (nextPos < myEndOffset)
                        {
                            char nextChar = Buffer[nextPos];
                            if (nextChar == '|' || nextChar == '\\')
                            {
                                _currentPosition += 2;
                                continue;
                            }

                            // else - common case
                        }
                    }
                    else if (Buffer[_currentPosition] == '|' || Buffer[_currentPosition] == '\n')
                    {
                        break;
                    }

                    _currentPosition++;
                }

                while (_currentPosition > 0 && char.IsWhiteSpace(Buffer[_currentPosition - 1]))
                {
                    _currentPosition--;
                }
            }
            else if (c == '#')
            {
                TokenType = GherkinTokenTypes.COMMENT;
                AdvanceToEol();

                string commentText = Buffer.GetText(new TextRange(myCurrentTokenStart + 1, _currentPosition)).Trim();
                string language = FetchLocationLanguage(commentText);
                if (language != null)
                {
                    UpdateLanguage(language);
                }
            }
            else if (c == ':')
            {
                TokenType = GherkinTokenTypes.COLON;
                _currentPosition++;
            }
            else if (c == '@')
            {
                TokenType = GherkinTokenTypes.TAG;
                _currentPosition++;
                while (_currentPosition < myEndOffset && IsValidTagChar(Buffer[_currentPosition]))
                {
                    _currentPosition++;
                }
            }
            else if (IsStringAtPosition(PYSTRING_MARKER))
            {
                TokenType = GherkinTokenTypes.PYSTRING;
                myState = STATE_INSIDE_PYSTRING;
                _currentPosition += 3;
            }
            else
            {
                if (myState == STATE_DEFAULT)
                {
                    foreach (var keyword in myKeywords)
                    {
                        int length = keyword.Length;
                        if (IsStringAtPosition(keyword))
                        {
                            if (myKeywordProvider.IsSpaceRequiredAfterKeyword(myCurLanguage, keyword) &&
                                myEndOffset - _currentPosition > length &&
                                char.IsLetterOrDigit(Buffer[_currentPosition + length]))
                            {
                                continue;
                            }

                            char followedByChar = _currentPosition + length < myEndOffset ? Buffer[_currentPosition + length] : (char)0;
                            TokenType = myKeywordProvider.GetTokenType(myCurLanguage, keyword);
                            if (TokenType == GherkinTokenTypes.STEP_KEYWORD)
                            {
                                bool followedByWhitespace = char.IsWhiteSpace(followedByChar) && followedByChar != '\n';
                                if (followedByWhitespace != myKeywordProvider.IsSpaceRequiredAfterKeyword(myCurLanguage, keyword))
                                {
                                    TokenType = GherkinTokenTypes.TEXT;
                                }
                            }

                            _currentPosition += length;
                            if (TokenType == GherkinTokenTypes.STEP_KEYWORD || TokenType == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD)
                            {
                                myState = STATE_AFTER_KEYWORD_WITH_PARAMETER;
                            }
                            else
                            {
                                myState = STATE_AFTER_KEYWORD;
                            }

                            return;
                        }
                    }
                }

                if (myState == STATE_PARAMETER_INSIDE_STEP)
                {
                    if (c == '>')
                    {
                        myState = STATE_AFTER_KEYWORD_WITH_PARAMETER;
                        _currentPosition++;
                        TokenType = GherkinTokenTypes.STEP_PARAMETER_BRACE;
                    }
                    else
                    {
                        AdvanceToParameterEnd("\n");
                        TokenType = GherkinTokenTypes.STEP_PARAMETER_TEXT;
                    }

                    return;
                }
                else if (myState == STATE_AFTER_KEYWORD_WITH_PARAMETER)
                {
                    if (_currentPosition < myEndOffset && Buffer[_currentPosition] == '<' && IsStepParameter("\n"))
                    {
                        myState = STATE_PARAMETER_INSIDE_STEP;
                        _currentPosition++;
                        TokenType = GherkinTokenTypes.STEP_PARAMETER_BRACE;
                    }
                    else
                    {
                        TokenType = GherkinTokenTypes.TEXT;
                        AdvanceToParameterOrSymbol("\n", STATE_AFTER_KEYWORD_WITH_PARAMETER, true);
                    }

                    return;
                }

                TokenType = GherkinTokenTypes.TEXT;
                AdvanceToEol();
            }
        }
        
        private static string FetchLocationLanguage(string commentText)
        {
            return commentText.StartsWith("language:") ? commentText.Substring(9).Trim() : null;
        }
        
        private void AdvanceOverWhitespace() {
            if (Buffer[_currentPosition] == '\n')
                myState = STATE_DEFAULT;

            _currentPosition++;
        }
        
        private bool IsStringAtPosition(string keyword) {
            int length = keyword.Length;
            return myEndOffset - _currentPosition >= length && Buffer.GetText(new TextRange(_currentPosition, _currentPosition + length)).Equals(keyword);
        }

        private bool IsStringAtPosition(string keyword, int position) {
            int length = keyword.Length;
            return myEndOffset - position >= length && Buffer.GetText(new TextRange(position, position + length)).Equals(keyword);
        }

        private static bool IsValidTagChar(char c) {
            return !char.IsWhiteSpace(c) && c != '@';
        }
        
        private void AdvanceToParameterEnd(string endSymbol) {
            _currentPosition++;
            int mark = _currentPosition;
            while (_currentPosition < myEndOffset && !IsStringAtPosition(endSymbol) && Buffer[_currentPosition] != '>') {
                _currentPosition++;
            }

            if (_currentPosition < myEndOffset) {
                if (IsStringAtPosition(endSymbol)) {
                    myState = STATE_DEFAULT;
                }
            }

            ReturnWhitespace(mark);
        }

        private void ReturnWhitespace(int mark)
        {
            while (_currentPosition > mark && char.IsWhiteSpace(Buffer[_currentPosition - 1]))
                _currentPosition--;
        }
        
        private void AdvanceToEol() {
            _currentPosition++;
            int mark = _currentPosition;
            while (_currentPosition < myEndOffset && Buffer[_currentPosition] != '\n') {
                _currentPosition++;
            }
            ReturnWhitespace(mark);
            myState = STATE_DEFAULT;
        }

        private void AdvanceToParameterOrSymbol(string s, int parameterState, bool shouldReturnWhitespace) {
            int mark = _currentPosition;

            while (_currentPosition < myEndOffset && !IsStringAtPosition(s) && !IsStepParameter(s)) {
                _currentPosition++;
            }

            if (shouldReturnWhitespace) {
                myState = STATE_DEFAULT;
                if (_currentPosition < myEndOffset) {
                    if (!IsStringAtPosition(s)) {
                        myState = parameterState;
                    }
                }

                ReturnWhitespace(mark);
            }
        }
        
        private bool IsStepParameter(string currentElementTerminator) {
            int pos = _currentPosition;

            if (Buffer[pos] == '<') {
                while (pos < myEndOffset && Buffer[pos] != '\n' && Buffer[pos] != '>' && !IsStringAtPosition(currentElementTerminator, pos)) {
                    pos++;
                }

                return pos < myEndOffset && Buffer[pos] == '>';
            }

            return false;
        }


        public object CurrentPosition { get; set; }
        
        public TokenNodeType TokenType { get; private set; }
        
        public int TokenStart => myCurrentTokenStart;
        
        public int TokenEnd => _currentPosition;
        
        public IBuffer Buffer { get; }
    }
}