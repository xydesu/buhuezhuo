using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class SongPlaying
    {
        enum TokenType
        {
            BPM,        // (120)
            Beats,      // {4}
            Note,       // 4, 3, etc.
            Rest,      // ,
            Slash,      // /
            NewLine,    // \n
            Comment,    // # ...
        }

        class PostInfo
        {
            public int Line { get; }
            public Range Range { get; }

            public PostInfo(int line, Range position)
            {
                Line = line;
                Range = position;
            }

            public override string ToString() => $"{Line}:{Range}";
        }

        class Token : PostInfo
        {
            public Token(TokenType type, string value, int line, Range position)
                : base(line, position)
            {
                Type = type;
                Value = value;
            }

            public Token(TokenType type, string value, int line, int start, int lan = 1)
                : this(type, value, line, new Range(start, start + lan)) { }

            public TokenType Type { get; }
            public string Value { get; }

            public override string ToString() => $"{Type}({Value}) at {base.ToString()}";
        }

        class ErrorPos : Exception
        {
            public PostInfo PositionInfo { get; }

            public ErrorPos(string message, int line, Range position)
                : base($"Error at {line}:{position} - {message}")
            {
                PositionInfo = new PostInfo(line, position);
            }

            public ErrorPos(string message, PostInfo posInfo)
                : base($"Error at {posInfo} - {message}")
            {
                PositionInfo = posInfo;
            }

            public ErrorPos(string message, int line, int start, int lan = 1)
                : this(message, line, new Range(start, start + lan))
            {
            }

            public override string ToString() => $"{Message} (at {PositionInfo})";
        }

        class Note
        {
            public int Lane { get; }
            public float Time { get; }

            public Note(int lane, float time)
            {
                Lane = lane;
                Time = time;
            }
        }

        static void PrintWarning(string input, ErrorPos warning)
        {
            var posInfo = warning.PositionInfo;
            var posRange = posInfo.Range;

            int errorStartPos = posRange.Start.Value;
            int errorEndPos = posRange.End.Value;
            string lineStr = input.Split('\n')[posInfo.Line];
            string errorStr = lineStr[errorStartPos..errorEndPos];

            int startPos = Math.Max(0, errorStartPos - 10);
            int endPos = Math.Min(lineStr.Length, posRange.End.Value + 10);
            int errorStartOffset = errorStartPos - startPos;

            string warningMessage = $"{warning.Message}:\n";
            if (startPos != 0)
            {
                warningMessage += "...";
                errorStartOffset += 3;
            }
            warningMessage += lineStr[startPos..errorStartPos];

            warningMessage += $"<color=yellow>{lineStr[errorStartPos..errorEndPos]}</color>";

            warningMessage += lineStr[errorEndPos..endPos];
            if (endPos < lineStr.Length) warningMessage += "...";
            warningMessage += "\n";

            string caret = new string(' ', errorStartOffset) + new string('^', errorStr.Length);
            warningMessage += caret;

            Debug.LogWarning(warningMessage);
        }

        private List<Token> LexicalAnalysis(string input, out List<ErrorPos> warnings)
        {
            warnings = new List<ErrorPos>();
            List<Token> tokens = new List<Token>();
            int line = 0, position = 0;

            for (int i = 0; i < input.Length; i++, position++)
            {
                char c = input[i];

                switch (c)
                {
                    case '(':
                        {
                            int endBpm = input.IndexOf(')', i);
                            if (endBpm == -1)
                            {
                                warnings.Add(new ErrorPos("Unclosed BPM token", line, position, Math.Max(input.Length, 10)));
                                break;
                            }

                            int len = endBpm - i;
                            tokens.Add(new Token(TokenType.BPM, input.Substring(i + 1, len - 1), line, position, len));
                            i = endBpm;
                            position += len;
                        }
                        break;

                    case '{':
                        {
                            int endBeats = input.IndexOf('}', i);
                            if (endBeats == -1)
                            {
                                warnings.Add(new ErrorPos("Unclosed Beats token", line, position, Math.Max(input.Length, 10)));
                                break;
                            }

                            int len = endBeats - i;
                            tokens.Add(new Token(TokenType.Beats, input.Substring(i + 1, len - 1), line, position, len));
                            i = endBeats;
                            position += len;
                        }
                        break;

                    case ',':
                        tokens.Add(new Token(TokenType.Rest, ",", line, position));
                        break;

                    case '/':
                        tokens.Add(new Token(TokenType.Slash, "/", line, position));
                        break;

                    case '\n':
                        tokens.Add(new Token(TokenType.NewLine, "\\n", line, position));
                        line++;
                        position = -1;
                        break;

                    case '#':
                        int endComment = input.IndexOf('\n', i);
                        string comment = endComment == -1
                            ? input[(i + 1)..]
                            : input.Substring(i + 1, endComment - i - 1);
                        tokens.Add(new Token(TokenType.Comment, comment, line, position));
                        i = endComment == -1 ? input.Length : endComment - 1;
                        break;

                    case ' ':
                        break;

                    default:
                        if (char.IsDigit(c))
                            tokens.Add(new Token(TokenType.Note, c.ToString(), line, position));
                        else
                            warnings.Add(new ErrorPos("Invalid character", line, position));
                        break;
                }
            }

            return tokens;
        }

        private List<Note> ParseTokens(List<Token> tokens, out List<ErrorPos> warnings)
        {
            warnings = new List<ErrorPos>();
            List<Note> notes = new List<Note>();
            HashSet<int> currentNotes = new HashSet<int>();

            decimal bpm = 120;
            int beatsPerMeasure = 4;
            decimal currentTime = 1.6m;

            Token? lastToken = null;

            foreach (var token in tokens)
            {
                if (token.Type != TokenType.Slash && token.Type != TokenType.Note) currentNotes.Clear();
                if (token.Type != TokenType.Note && lastToken?.Type == TokenType.Slash)
                    warnings.Add(new ErrorPos("Slash without note", token.Line, token.Range));

                switch (token.Type)
                {
                    case TokenType.BPM: // (120)
                        if (decimal.TryParse(token.Value, out decimal bpmValue))
                        {
                            if (bpmValue < 0) warnings.Add(new ErrorPos("BPM cannot be negative", token.Line, token.Range));
                            else if (bpmValue == 0) warnings.Add(new ErrorPos("BPM cannot be 0", token.Line, token.Range));
                            else bpm = bpmValue;
                        }
                        else warnings.Add(new ErrorPos("Invalid BPM", token.Line, token.Range));
                        break;

                    case TokenType.Beats: // {<value>}
                        if (decimal.TryParse(token.Value, out decimal beatsPerMeasureValue))
                        {
                            if (beatsPerMeasureValue % 1 != 0)
                                warnings.Add(new ErrorPos("Invalid beats per measure, must be an integer", token.Line, token.Range));
                            else if (beatsPerMeasureValue < 1)
                                warnings.Add(new ErrorPos("Invalid beats per measure, must be at least 1", token.Line, token.Range));
                            else if (beatsPerMeasureValue == 0)
                                warnings.Add(new ErrorPos("Invalid beats per measure, cannot be 0", token.Line, token.Range));
                            else beatsPerMeasure = (int)beatsPerMeasureValue;
                        }
                        else
                            warnings.Add(new ErrorPos("Invalid beats per measure", token.Line, token.Range));
                        break;

                    case TokenType.Slash: // /
                        if (lastToken?.Type == TokenType.Slash)
                            warnings.Add(new ErrorPos("Double slash", token.Line, token.Range));
                        break;

                    case TokenType.Rest: // ,
                        currentTime += 60m / bpm * (4m / beatsPerMeasure);
                        break;

                    case TokenType.Note: // number
                        if (int.TryParse(token.Value, out int lane))
                        {
                            if (lane < 1 || lane > 4)
                                warnings.Add(new ErrorPos("Invalid note", token.Line, token.Range));
                            if (!currentNotes.Add(lane))
                                warnings.Add(new ErrorPos("Duplicate note", token.Line, token.Range));
                            else notes.Add(new Note(lane, (float)currentTime));
                        }
                        else
                            warnings.Add(new ErrorPos("Invalid note", token.Line, token.Range));
                        break;

                    default:
                        // Ignore other token types
                        break;
                }

                lastToken = token;
            }

            return notes;
        }
    }
}
