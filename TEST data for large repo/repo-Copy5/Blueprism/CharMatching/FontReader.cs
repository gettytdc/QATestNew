using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BluePrism.BPCoreLib;
using System.Reflection;
using System.Globalization;
using System.Text;
using NLog;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Recognition of text from a bitmap, based on pre-defined font data.
    /// </summary>
    public static class FontReader
    {
        #region - Static Variables -

        // The font store used within the font reader class
        private static IFontStore _fontStore = null;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region - ReadText (Legacy Method) -

        /// <summary>
        /// Read text from a PixRect using font recognition.
        /// </summary>
        /// <param name="useBg">True to take the given colour and use it as the
        /// background colour; False to treat it as the foreground colour.</param>
        /// <param name="pic">The picture to read the text from</param>
        /// <param name="fontName">The name of the installed font to use to recognise
        /// the text.</param>
        /// <param name="col">The colour of the text or background to distinguish
        /// between them - this should be in RRGGBB format.</param>
        /// <returns>The text read from the image</returns>
        /// <remarks>This is the original Ciaran font recognition algorithm.</remarks>
        /// <exception cref="NoSuchFontException">If the given font name was not
        /// recognised as a font in the currently installed font store</exception>
        /// <exception cref="EmptyFontException">If the given font contained no
        /// character data</exception>
        /// <exception cref="Exception">If any other errors occur while attempting to
        /// read the text</exception>
        public static string ReadText(
            bool useBg, clsPixRect pic, string fontName, int col)
        {
            return ReadText(useBg, pic, GetFontData(fontName), col);
        }

        /// <summary>
        /// Read text from a PixRect using font recognition.
        /// </summary>
        /// <param name="useBg">True to take the given colour and use it as the
        /// background colour; False to treat it as the foreground colour.</param>
        /// <param name="pic">The picture to read the text from</param>
        /// <param name="font">The font data to use to recognise the text.</param>
        /// <param name="col">The colour of the text or background to distinguish
        /// between them - this should be in RRGGBB format.</param>
        /// <returns>The text read from the image</returns>
        /// <exception cref="Exception">If any errors occur while attempting to
        /// read the text</exception>
        /// <remarks>This is the original Ciaran font recognition algorithm</remarks>
        public static string ReadText(
            bool useBg, clsPixRect pic, FontData font, int col)
        {
            CharCanvas canv = useBg
                ? CharCanvas.FromPixRectByBackgroundColour(pic, col, false)
                : CharCanvas.FromPixRectByForegroundColour(pic, col);

            // Find the characters...
            string c = CharData.NullValue;
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y <= pic.Height - 1; y++)
            {
                for (int x = 0; x <= pic.Width - 1; x++)
                {
                    int cw;
                    c = font.GetTextValueAtPosition(new Point(x, y), canv, out cw);
                    if (c != CharData.NullValue)
                    {
                        sb.Append(c);
                        // Skip past the text value, but add one less than the
                        // width because the for loop will increment it anyway.
                        // Note that we must allow for there being no gap between
                        // the two text values
                        x += cw - 1;
                    }
                }
            }
            return sb.ToString();
        }

        #endregion

        #region - ReadTextSingleLine & CharacterMatch inner class -

        /// <summary>
        /// Records information about a character matched at a particular location
        /// on the canvas.
        /// </summary>
        private class CharacterMatch : IComparable
        {
            /// <summary>
            /// The character found.
            /// </summary>
            public CharData Character;

            /// <summary>
            /// The zero-based index of the row at which the match was made. The
            /// top of the character starts here.
            /// </summary>
            public int YPos;

            /// <summary>
            /// Indicates whether the match was strict, in the sense that 100% of
            /// both the character's black and white sets of pixels were all matched.
            /// This may not be the case in studying a font with tight kerning.
            /// </summary>
            public bool NonStrictMatch;

            public CharacterMatch(CharData Character, int YPos, bool NonStrictMatch)
            {
                this.Character = Character;
                this.YPos = YPos;
                this.NonStrictMatch = NonStrictMatch;
            }

            public int CompareTo(object obj)
            {
                CharacterMatch cm = obj as CharacterMatch;
                return (cm == null ? 1 : Character.CompareTo(cm.Character));
            }
        }

        /// <summary>
        /// Read text from a PixRect using font recognition, working on the assumption
        /// that all text resides on the same line.
        /// Spaces are inserted between words, where the font carries space and
        /// kerning data.
        /// </summary>
        /// <param name="useBg">If true, the text is identified by means
        /// of the background colour; otherwise the supplied colour is interpreted
        /// as the foreground text colour.</param>
        /// <param name="pic">The pixrect to read from</param>
        /// <param name="font">The font to be used.</param>
        /// <param name="col">The colour of the text to read. RRGGBB.</param>
        /// <returns>The text read from the image</returns>
        /// <exception cref="Exception">If any errors occur while attempting to read
        /// the text in the image.</exception>
        public static string ReadTextSingleLine(
            bool useBg, clsPixRect pic, string fontName, int col)
        {
            return ReadTextSingleLine(useBg, pic, GetFontData(fontName), col);
        }

        /// <summary>
        /// Read text from a PixRect using font recognition, working on the assumption
        /// that all text resides on the same line.
        /// Spaces are inserted between words, where the font carries space and
        /// kerning data.
        /// </summary>
        /// <param name="useBg">If true, the text is identified by means
        /// of the background colour; otherwise the supplied colour is interpreted
        /// as the foreground text colour.</param>
        /// <param name="pic">The pixrect to read from</param>
        /// <param name="font">The font to be used.</param>
        /// <param name="col">The colour of the text to read. RRGGBB.</param>
        /// <returns>The text read from the image</returns>
        /// <exception cref="Exception">If any errors occur while attempting to read
        /// the text in the image.</exception>
        public static string ReadTextSingleLine(
            bool useBg, clsPixRect pic, FontData font, int col)
        {
            CharCanvas canv;
            if (useBg)
                canv = CharCanvas.FromPixRectByBackgroundColour(pic, col, false);
            else
                canv = CharCanvas.FromPixRectByForegroundColour(pic, col);

            return ReadTextSingleLine(canv, font);
        }

        /// <summary>
        /// Read text from a canvas using font recognition, working on the assumption
        /// that all text resides on the same line.
        /// </summary>
        /// <param name="canv">The canvas from which to read the text.</param>
        /// <param name="fontData">The font to use when reading the text.</param>
        /// <returns>The line of text read from the given canvas using the specified
        /// font.</returns>
        /// <exception cref="Exception">If any errors occur while attempting to
        /// read the text.</exception>
        private static string ReadTextSingleLine(CharCanvas canv, FontData fontData)
        {
            int topMargin = fontData.MaxWhiteSpaceAbove;
            int bottomMargin = fontData.MaxWhiteSpaceBelow;

            canv = canv.TrimVertical(topMargin, bottomMargin,
                Math.Max(fontData.Height, 14) + topMargin + bottomMargin);

            // Note that the vertical trimming has been, er, trimmed - see bug 6949
            canv = canv.TrimHorizontal();

            int leftEdge = 0;
            string sOutText = "";
            CharData lastChar = null;
            int lastX = -1;
            int lastY = -1;

            //We figure out where the blank columns are between characters, for
            //the purpose of identifying where spaces are. We do this up front
            //because we later delete characters from the canvas, creating new
            //blank space which would otherwise confuse things
            Dictionary<int, bool> blankCol = new Dictionary<int, bool>();
            for (int x = 0; x <= canv.Width - 1; x++)
            {
                blankCol.Add(x, canv.IsColumnEmpty(x));
            }
        Rebegin:

            int skipCount = 0;

            for (int x = leftEdge; x < canv.Width; x++)
            {
                if (blankCol[x])
                {
                    skipCount++;
                    continue;
                }
                else
                {
                    List<CharacterMatch> matches = new List<CharacterMatch>();
                    for (int y = 0; y <= canv.Height - 1; y++)
                    {
                        bool nonStrict = false;
                        CharData cc = fontData.GetCharAtPosition(
                            new Point(x, y), canv, out nonStrict, false, false);
                        if (cc != null)
                        {
                            //Only accept characters if they match the previous y value
                            if (lastY == -1 || y == lastY)
                            {
                                matches.Add(new CharacterMatch(cc, y, nonStrict));
                            }
                        }
                    }

                    if (matches.Count > 1)
                    {
                        //We prefer characters found at the same y-pos as last time.
                        //Therefore remove all characters not situated at the
                        //expected y position, as long as there is at least one.
                        if (lastY > -1)
                        {
                            foreach (CharacterMatch match in matches)
                            {
                                if (match.YPos == lastY)
                                {
                                    for (int i = matches.Count - 1; i >= 0; i += -1)
                                    {
                                        if (matches[i].YPos != lastY)
                                            matches.RemoveAt(i);
                                    }
                                    break;
                                }
                            }
                        }
                        matches.Sort();
                    }

                    if (matches.Count > 0)
                    {
                        CharacterMatch largestMatch = matches[0];
                        CharData matchChar = largestMatch.Character;

                        //Sometimes this character can be BEHIND the last char found.
                        //Eg Lucida Sans Unicode 10 Italic - the tail of the S sticks
                        //out a long way behind, so the 'S combo has the apostrophe
                        //close up to the S with the tail sticking out behind.
                        //We take care not to introduce additional spaces due to our
                        //backtracking
                        if (lastX > -1)
                        {
                            if (lastChar != null)
                            {
                                var interCharacterSpace = x - lastX - lastChar.Width;

                                if (interCharacterSpace < 0)
                                {
                                    skipCount = 0;
                                }

                                skipCount = Math.Min(skipCount, interCharacterSpace);
                            }
                        }

                        if (lastChar != null && fontData.SpaceWidth > 0)
                        {
                            //Figure out how many spaces we should add, based on:
                            // 1) what the kerning would be between these two characters;
                            // 2) how much extra space there is; and
                            // 3) how much space we have identified.
                            //This is complicated somewhat by the possibility that we may have read some of
                            //our characters incorrectly due to two characters having the same appearance
                            //(ie the l vs I problem). We have to evaluate each possibility.
                            int maxSpaces = -1;
                            foreach (CharData ch1 in fontData.GetCharacterAlternatives(lastChar))
                            {
                                foreach (CharData ch2 in fontData.GetCharacterAlternatives(matchChar))
                                {
                                    int kernWidth = fontData.GetKernValue(ch1, ch2);

                                    if (skipCount > kernWidth)
                                    {
                                        //Note that kernwidth can be negative - it is important to "add"
                                        //on the negatively kerned values to get the full effective gap.
                                        //For most fonts, any positive kerned gap does not usually exist in addition
                                        //to the usual "space" character gap.
                                        int effvSkip = skipCount - Math.Min(0, kernWidth);
                                        maxSpaces = Math.Max(maxSpaces, effvSkip / fontData.SpaceWidth);
                                    }
                                }
                            }

                            for (int i = 1; i <= maxSpaces; i++)
                            {
                                sOutText += " ";
                            }
                        }

                        sOutText += matchChar.Value;
                        lastChar = matchChar;
                        lastY = largestMatch.YPos;
                        lastX = x;

                        //Delete the character we just read
                        canv = canv.DeletePoints(
                            matchChar.GetCanvasPointsOfCharInk(
                            new Point(x, largestMatch.YPos), canv));

                        //Wipe out any "blank" columns contained inside the character's
                        //bounds. Eg there can be a blank stripe inside a double quote
                        //character.
                        for (int i = x; i <= x + matchChar.Width - 1; i++)
                        {
                            blankCol[i] = false;
                        }

                        //Skip past the character. Note that we allow for there
                        //being no gap between the two characters, which happens in
                        //Customer System sometimes
                        leftEdge = x + lastChar.Width;

                        if (largestMatch != null && largestMatch.NonStrictMatch)
                        {
                            int kernOffset = Math.Min(0, fontData.GetMinKerningValueAfter(lastChar.Value));
                            leftEdge = Math.Max(0, leftEdge + kernOffset);
                        }
                        else
                        {
                            //Experimental. Arial Narrow 10 Italic. The /\ combination touch
                            //at the peak, so we need to backtrack by one, even though the match
                            //is a strict one.
                            leftEdge -= 1;
                        }

                        skipCount = 0;
                        goto Rebegin;
                    }
                }
            }

            return sOutText;

        }

        #endregion

        #region - ReadTextMultiline -

        /// <summary>
        /// Read text from a PixRect using font recognition.
        /// Carriage returns are inserted where text appears on separate lines,
        /// and spaces are inserted between words, where the font carries space and
        /// kerning data.
        /// </summary>
        /// <param name="useBg">If true, the text is identified by means
        /// of the background colour; otherwise the supplied colour is interpreted
        /// as the foreground text colour.</param>
        /// <param name="pic">The pixrect to read from</param>
        /// <param name="fontName">The name of the font to be used.</param>
        /// <param name="col">The colour of the text to read (RRGGBB) or of the
        /// background to discard, depending on <paramref name="useBg"/>. </param>
        /// <param name="erase">True to erase blocks from the background matching;
        /// false otherwise.</param>
        /// <returns>THe lines of text read from the image</returns>
        /// <exception cref="NoSuchFontException">If no font with the given
        /// <param name="fontName">name</param> could be found in this font reader.
        /// </exception>
        /// <exception cref="EmptyFontException">If the font found with the given
        /// name had no characters defined within it</exception>
        /// <exception cref="ArgumentException">If was <param name="eraseBlocks"/>
        /// true and <param name="useBg"/> was false. "Erase Blocks" functionality
        /// only holds meaning when reading via the background colour.</exception>
        /// <exception cref="Exception">If any other errors occur.</exception>
        public static string ReadTextMultiline(bool useBg,
            clsPixRect pic, string fontName, int col, bool erase)
        {
            if (erase & !useBg)
                throw new ArgumentException(Resources.EraseBlocksParameterCannotBeUsedUnlessIdentifyingByBackgroundColour);

            //Get the font data...
            FontData font = GetFontData(fontName);

            CharCanvas canv;
            if (useBg)
                canv = CharCanvas.FromPixRectByBackgroundColour(pic, col, erase);
            else
                canv = CharCanvas.FromPixRectByForegroundColour(pic, col);

            // OK, we have the font; we have the canvase.
            // Split the lines and read each one using single-line matching

            StringBuilder sOut = new StringBuilder();
            bool first = true;

            foreach (CharCanvas line in SplitSample(canv, font.LineBreakThreshold))
            {
                if (!first)
                    sOut.AppendLine();
                else
                    first = false;

                sOut.Append(ReadTextSingleLine(line, font));
            }

            return sOut.ToString();
        }

        #endregion

        #region - Other public methods -

        /// <summary>
        /// Sets the font store to use for all future calls of the FontReader class.
        /// </summary>
        /// <param name="store">The store to use to read the font data for subsequent
        /// calls to methods on this class.</param>
        public static void SetFontStore(IFontStore store)
        {
            _fontStore = store;
        }

        /// <summary>
        /// Get the data for the specified font.
        /// It attempts to retrieve the data from the database, then from a file in
        /// the <see cref="FontConfig.Directory"/> called 'font_[name].xml'. Finally,
        /// it attempts to find the file in the assembly itself under the name
        /// 'BluePrism.CharMatching.EmbeddedFonts.[name].xml'.
        /// </summary>
        /// <param name="fontName">The name of the font to get the data for </param>
        /// <returns>The FontData found for the given name</returns>
        /// <exception cref="NoSuchFontException">If the font was not found, neither
        /// in the database, nor a file, nor embedded into this assembly.</exception>
        /// <exception cref="EmptyFontException">If the returned font data had no
        /// characters defined within it</exception>
        public static FontData GetFontData(string fontName)
        {
            FontData data = null;
            try
            {
                // Next try getting it from the font store
                if (_fontStore != null)
                {
                    try
                    {
                        BPFont f = _fontStore.GetFont(fontName);
                        if (f != null)
                            data = f.Data;
                    }
                    // Nothing we can do about exceptions other than to try a file.
                    catch { }
                }

                // If not there, attempt to load direct from XML - either a file
                // in the install directory or an XML file embedded in the
                // (executing) assembly
                if (data == null)
                {
                    // Look for a font data file in the user font directory first.
                    // This allows users to install additional font support on the
                    // fly, and also since we are doing it, the compiled-in data can
                    // be overridden in an emergency...
                    string file = Path.Combine(
                        FontConfig.Directory, "font_" + fontName + ".xml");

                    if (File.Exists(file))
                    {
                        using (var reader = new StreamReader(file))
                        {
                            data = new FontData(reader.ReadToEnd());
                        }

                    }
                    else
                    {
                        // Ok then, get the font from the embedded XML resource...
                        Assembly asm = Assembly.GetExecutingAssembly();
                        Stream stream = asm.GetManifestResourceStream(string.Format(
                            "BluePrism.CharMatching.EmbeddedFonts.{0}.xml",
                            fontName));
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                data = new FontData(reader.ReadToEnd());
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

            // If there's no font data, indicate as much
            if (data == null)
                throw new NoSuchFontException(fontName);

            // If there's no characters well, that's bad too
            if (data.Characters.Count == 0)
                throw new EmptyFontException(fontName);

            // Otherwise, all is well
            return data;
        }

        public static string GetFontDataOcrPlus(string fontName)
        {
            string data = string.Empty;
            try
            {
                // Next try getting it from the font store
                if (_fontStore != null)
                {
                    try
                    {
                        data = _fontStore.GetFontOcrPlus(fontName);
                    }
                    catch
                    {
                        data = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

            // If there's no font data, indicate as much
            if (data == string.Empty)
                throw new NoSuchFontException(fontName);

            return data;
        }

        /// <summary>
        /// Attempts to parse the given string into a colour, interpreting it either
        /// as a <see cref="Color.FromName">named .NET colour</see> or as a hex
        /// number in the format 'RRGGBB', with 2 hex digits for each colour
        /// component.
        /// If string is not there (ie. null or empty), this outputs -1 to indicate
        /// that no colour was present in the string; this is considered a valid
        /// input, however. If the string is non-empty but does not contain either a
        /// named .NET colour or a hex number in the above format (optionally
        /// preceded by a single '#'), this will return false to indicate an invalid
        /// value.
        /// </summary>
        /// <param name="str">The string to try and parse into a colour.</param>
        /// <param name="colour">The integer RGB value representing the colour
        /// derived from the given string, or -1 if no colour could be derived from
        /// the string (either it was empty or the string did not represent a valid
        /// colour)</param>
        /// <returns>True if the given string was considered a valid colour value;
        /// either by virtue of being empty and thus representing 'no colour set', or
        /// containing a hex value which could be converted into a colour.</returns>
        public static bool TryParseColourString(string str, out int colour)
        {
            colour = -1; // default 'no colour there' value

            // This is a valid 'no colour given' value. The colour value of -1
            // indicates that the colour is not set, return that it was 'valid',
            // even if that valid value was 'not present'.
            if (str == null || (str = str.Trim()).Length == 0)
                return true;

            // We support named colours as well
            Color col = Color.FromName(str);
            if (col.IsKnownColor)
            {
                // We could possibly use ToArgb(), but we want to be assured of
                // discarding the alpha part of the colour
                colour = col.R << 16 | col.G << 8 | col.B;
                return true;
            }

            // If we have a 7 char string starting with a '#', just strip the
            // '#' from it and try that - it's how it's done in HTML; there's no
            // reason not to handle it gracefully
            if (str.Length == 7 && str[0] == '#')
                str = str.Substring(1, 6);

            // if the string we have at this point is not 6 chars, we can error
            // straight away. That's an invalid value, so tell the caller as such
            if (str.Length != 6)
                return false;

            try
            {
                colour = int.Parse(str, NumberStyles.HexNumber);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the given string into a colour, interpreting it either as a
        /// <see cref="Color.FromName">named .NET colour</see> or as a hex number in
        /// the format 'RRGGBB', with 2 hex digits for each colour component.
        /// If string is not there (ie. null or empty), this outputs -1 to indicate
        /// that no colour was present in the string; this is considered a valid
        /// input, however. If the string is non-empty but does not contain either a
        /// named .NET colour or a hex number in the above format (optionally
        /// preceded by a single '#'), this will return false to indicate an invalid
        /// value.
        /// </summary>
        /// <param name="colStr">The string representation of the colour either as a
        /// named .NET colour or in RRGGBB format. The alpha channel is assumed to be
        /// "full" - ie FF. This may be null or empty.</param>
        /// <returns>Returns the corresponding integer representation of the 
        /// colour. If a null or empty string value is supplied, then -1 returned.
        /// </returns>
        /// <exception cref="ArgumentException">If the given colour string could not
        /// be parsed into a colour.</exception>
        public static int ParseColourString(string colStr)
        {
            int col;
            if (TryParseColourString(colStr, out col))
                return col;

            throw new ArgumentException(
                    Resources.ANamedColourOrASixDigitColourStringIsRequired);
        }

        #endregion

        #region - Other private methods -

        /// <summary>
        /// Splits canvas sample by locating rows which are entirely blank.
        /// </summary>
        /// <param name="canv">The canvas to be broken up.</param>
        /// <param name="lineBreakThreshold">The number of rows of blank pixels that
        /// should be observed before cutting to a new line.</param>
        /// <returns>Returns a list of canvases, corresponding to different lines 
        /// of text found. Each line includes the extraneous
        /// white space above and below the each line, such that if you were
        /// to reconstruct the original image, you would have to overlap the white
        /// space between each segment.</returns>
        private static List<CharCanvas> SplitSample(
            CharCanvas canv, int lineBreakThreshold)
        {

            //Scan each row, recording true/false for blank/not blank
            List<bool> RowBlanks = new List<bool>();
            for (int y = 0; y <= canv.Height - 1; y++)
            {
                bool IsRowBlank = true;
                for (int x = 0; x <= canv.Width - 1; x++)
                {
                    if (canv[x, y] == CharCanvasState.InkExists)
                    {
                        IsRowBlank = false;
                        break;
                    }
                }
                RowBlanks.Add(IsRowBlank);
            }

            //We operate an FSA with states:
            // 0 - uninitialised
            // 1 - Within a segment, but only seen whites so far
            // 2 - Within a segment, and have seen some blacks
            // 3 - Within a segment, seen some blacks and at least one white, but
            //     have not yet met the threshold for consecutive whites.
            // 4 - Within a segment, have seen enough whites to meet the threshold.
            //     We will end the current segment at the next black
            int State = 0;
            int CurrentSegmentStartIndex = -1;
            int NextSegmentStartIndex = -1;
            List<CharCanvas> FoundLines = new List<CharCanvas>();
            int Index = 0;
            int BlankLinesEncountered = 0;
            // A running tally of how many blank lines we have encountered within state 4
            foreach (bool RowIsBlank in RowBlanks)
            {
                switch (State)
                {
                    case 0:
                        //The uninitialised state, only ever happens on first row
                        CurrentSegmentStartIndex = 0;
                        if (!RowIsBlank)
                        {
                            State = 2;
                        }
                        else
                        {
                            State = 1;
                        }
                        break;
                    case 1:
                        //We are in a segment and only seen white rows so far
                        if (!RowIsBlank)
                        {
                            State = 2;
                        }
                        else
                        {
                            //no change
                        }
                        break;
                    case 2:
                        //We are in a segment and have seen at least one black row
                        if (!RowIsBlank)
                        {
                            //no change
                        }
                        else
                        {
                            if (lineBreakThreshold == 1)
                            {
                                //We have now resolved to end this segment. The next segment will begin here
                                NextSegmentStartIndex = Index;
                                State = 4;
                            }
                            else
                            {
                                State = 3;
                                BlankLinesEncountered = 1;
                            }
                        }
                        break;
                    case 3:
                        //We are in a segment and have seen some consecutive whites, but not
                        //enough yet to breach the threshold.
                        if (!RowIsBlank)
                        {
                            State = 2;
                            BlankLinesEncountered = 0;
                        }
                        else
                        {
                            BlankLinesEncountered += 1;
                            if (BlankLinesEncountered >= lineBreakThreshold)
                            {
                                //We have now resolved to end this segment. The next segment will begin
                                //where the first blank was encountered.
                                NextSegmentStartIndex = Index - (lineBreakThreshold - 1);
                                State = 4;
                            }
                            else
                            {
                                //continue scanning
                            }
                        }
                        break;
                    case 4:
                        //We are just waiting to end the current segment. This will happen when we encounter the
                        //next black row
                        if (!RowIsBlank)
                        {
                            //Segment content ends on previous row
                            FoundLines.Add(canv.CopyRows(CurrentSegmentStartIndex, Index - 1));

                            //We now enter a new segment, which starts at the point where we saw the first blank row
                            CurrentSegmentStartIndex = NextSegmentStartIndex;
                            State = 1;
                        }
                        else
                        {
                            //no change
                        }
                        break;
                }

                Index += 1;
            }

            //Save any half-finished segments
            switch (State)
            {
                case 0:
                    FoundLines.Add(canv);
                    break;
                case 1:
                    break;
                //Only seen whites so not bothered
                case 2:
                case 3:
                case 4:
                    FoundLines.Add(canv.CopyRows(CurrentSegmentStartIndex, canv.Height - 1));
                    break;
            }

            return FoundLines;
        }

        #endregion

    }
}
