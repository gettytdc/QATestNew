using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using BluePrism.BPCoreLib;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;


namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class with which a Blue Prism font can be extracted from a system font.
    /// </summary>
    public class SystemFontExtractor
    {
        const string ocrPlusExe = @"OcrPlus\bpocrpp.exe";

        #region - Static Extract Methods -

        /// <summary>
        /// Extracts a Blue Prism font from the given system font, not saving it
        /// first
        /// </summary>
        /// <param name="f">The font to extract</param>
        /// <param name="name">The proposed name of the font</param>
        /// <param name="store">The font store into which the generated font will be
        /// stored. Note that this method doesn't save the font, but it may use the
        /// font to ensure that there are no duplicate font names in the store as a
        /// result of this extraction.</param>
        /// <param name="mon">A progress monitor to which progress can be recorded
        /// </param>
        /// <returns>The extracted Blue Prism font</returns>
        public static BPFont Extract(
            Font f, string name, IFontStore store, clsProgressMonitor mon)
        {
            SystemFontExtractor fe = new SystemFontExtractor();
            fe.FontStore = store;
            return fe.GenerateFont(f, name, mon);
        }

        /// <summary>
        /// Extracts a Blue Prism font from the given system font, saving it into the
        /// specified store first.
        /// </summary>
        /// <param name="f">The font to extract</param>
        /// <param name="name">The proposed name of the font</param>
        /// <param name="store">The font store into which the generated font will be
        /// stored.</param>
        /// <param name="mon">A progress monitor to which progress can be recorded
        /// </param>
        /// <returns>The extracted Blue Prism font</returns>
        public static BPFont ExtractAndSave(
            Font f, string name, IFontStore store, clsProgressMonitor mon, bool ocrplus)
        {
            BPFont bpf = Extract(f, name, store, mon);
            if (mon.IsCancelRequested)
                return null;

            store.SaveFont(bpf);

            if (ocrplus)
            {
                mon.FireProgressChange(1, string.Format(Resources.GenerateABluePrismOCRPlusFont, name));
                BPFont obrplusFont = SystemFontExtractorOcrPlus.GenerateFont(f, name, mon);
                if (mon.IsCancelRequested)
                    return null;
                ConvertToOcrPlusJsonFont(f, obrplusFont, store);
            }

            mon.FireProgressChange(99, string.Format(Resources.SavingBluePrismFont0, name));
            return bpf;
        }

        private static void ConvertToOcrPlusJsonFont(Font f, BPFont obrplusFont, IFontStore store)
        {
            string temp = System.IO.Path.GetTempPath() + "ocrplusfonts";
            temp = temp + "\\" + f.FontFamily.Name;
            System.IO.Directory.CreateDirectory(temp);
            obrplusFont.Name = obrplusFont.Name.Replace("(", "");
            obrplusFont.Name = obrplusFont.Name.Replace(")", "");
            string fileName = temp + "\\" + obrplusFont.Name + ".bpfont";
            obrplusFont.ExportData(fileName);

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + ocrPlusExe;
            startInfo.Arguments = $" --fonts-directory \"{temp}\" --bpfont \"{fileName}\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            var ocrProcess = Process.Start(startInfo);
            ocrProcess.WaitForExit();
            ocrProcess.Close();

            string jsonFileName = temp + "\\" + f.FontFamily.Name + "\\" + obrplusFont.Name + ".bpfont.json";
            var json = System.IO.File.ReadAllText(jsonFileName);

            store.SaveFontOcrPlus(obrplusFont.Name, json);
        }

        #endregion

        #region - Member Variables -

        // The font store used for fonts used by this object
        private IFontStore _store;

        #endregion

        #region - Properties -

        /// <summary>
        /// The store to and from which fonts can be stored and retrieved
        /// respectively
        /// </summary>
        public IFontStore FontStore
        {
            get { return _store; }
            set { _store = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Trims any non-active "white" columns from the left and right hand side
        /// of the supplied image.
        /// </summary>
        /// <param name="BM">The image to be trimmed.</param>
        /// <param name="ActiveColour">The active (text) colour.</param>
        private void AutoTrimImageSides(ref Bitmap BM, Color ActiveColour)
        {
            int FirstNonEmptyColumn = -1;
            for (int x = 0; x <= BM.Width - 1; x++)
            {
                bool Empty = true;
                for (int y = 0; y <= BM.Height - 1; y++)
                {
                    if (BM.GetPixel(x, y).ToArgb() == ActiveColour.ToArgb())
                    {
                        Empty = false;
                        break;
                    }
                }
                if (!Empty)
                {
                    FirstNonEmptyColumn = x;
                    break;
                }
            }

            int LastNonEmptyColumn = int.MaxValue;
            for (int x = BM.Width - 1; x >= 0; x += -1)
            {
                bool Empty = true;
                for (int y = 0; y <= BM.Height - 1; y++)
                {
                    if (BM.GetPixel(x, y).ToArgb() == ActiveColour.ToArgb())
                    {
                        Empty = false;
                        break;
                    }
                }
                if (!Empty)
                {
                    LastNonEmptyColumn = x;
                    break;
                }
            }

            if (LastNonEmptyColumn < BM.Height || FirstNonEmptyColumn > 0)
            {
                int NewWidth = LastNonEmptyColumn - FirstNonEmptyColumn + 1;
                BM = BM.Clone(new Rectangle(FirstNonEmptyColumn, 0, NewWidth, BM.Height), BM.PixelFormat);
            }
        }

        /// <summary>
        /// Adds the given character from the given windows font into the specified
        /// BP FontData object.
        /// </summary>
        /// <param name="f">The BP Font to which the character should be added.
        /// </param>
        /// <param name="winFont">The windows font from which the character should
        /// be drawn.</param>
        /// <param name="val">The text value to add.</param>
        /// <param name="sf">The string format flags used to draw the character
        /// </param>
        private void AddCharacterToFont(
            FontData f, Font winFont, string val, StringFormat sf)
        {
            Bitmap b = new Bitmap(60, 60);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Color.White);
                g.DrawString(val.ToString(), winFont, Brushes.Black, 1, 1, sf);
                AutoTrimImageSides(ref b, Color.Black);
                f.AddCharacter(new CharData(val, b), true);
            }

            // We're done with the bitmap now - be rid.
            b.Dispose();
        }

        /// <summary>
        /// Gets the width of a space character in the given font with the specified
        /// string format
        /// </summary>
        /// <param name="f">The font for which the space width is required</param>
        /// <param name="fmt">The string formatting values for the font</param>
        /// <returns>The typical number of pixels wide that a space</returns>
        private int GetSpaceWidth(Font f, StringFormat fmt)
        {
            //Determine the typical kerning width
            const string KernText = "ll";
            Bitmap rubbish = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(rubbish);
            SizeF s = g.MeasureString(KernText, f, new Size(int.MaxValue, int.MaxValue), fmt);
            Bitmap bmp = new Bitmap(Convert.ToInt32(Math.Ceiling(s.Width)) + 4, Convert.ToInt32(Math.Ceiling(s.Height)) + 2);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.DrawString(KernText, f, Brushes.Black, 1, 1, fmt);
            int midY = bmp.Height / 2;
            int first = -1;
            int second = -1;
            bool hadSpace = false;
            for (int i = 0; i <= bmp.Width - 1; i++)
            {
                if (bmp.GetPixel(i, midY).ToArgb() == Color.Black.ToArgb())
                {
                    if (!hadSpace)
                    {
                        first = i;
                    }
                    else
                    {
                        if (hadSpace)
                        {
                            second = i;
                            break;
                        }
                    }
                }
                else
                {
                    if (first != -1)
                        hadSpace = true;
                }
            }
            int twoLKern = second - first - 1;
            AutoTrimImageSides(ref bmp, Color.Black);
            int twoLKernImageWidth = bmp.Width;

            //Determine the typical space width
            const string SpaceText = "l l";
            rubbish = new Bitmap(1, 1);
            g = Graphics.FromImage(rubbish);
            s = g.MeasureString(SpaceText, f, new Size(int.MaxValue, int.MaxValue), fmt);
            bmp = new Bitmap(Convert.ToInt32(Math.Ceiling(s.Width)) + 4, Convert.ToInt32(Math.Ceiling(s.Height)) + 2);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.DrawString(SpaceText, f, Brushes.Black, 1, 1, fmt);
            midY = bmp.Height / 2;
            first = -1;
            second = -1;
            AutoTrimImageSides(ref bmp, Color.Black);

            return bmp.Width - twoLKernImageWidth;
        }

        /// <summary>
        /// Generates a sensible name for a Blue Prism font which represents the
        /// given system font. This will ensure that it is unique within the store
        /// set in this extractor object
        /// </summary>
        /// <param name="f">The font for which a name should be generated</param>
        /// <returns>A name for the given font, not present within the store set in
        /// this object.</returns>
        public string GenerateName(Font f)
        {
            //Generate a name...
            string basename = string.Format("{0} {1}",
                f.FontFamily.Name, f.Size);
            if (f.Bold)
                basename += " Bold";
            if (f.Italic)
                basename += " Italic";
            if (f.Underline)
                basename += " Underlined";

            //Make sure it's unique...
            string name = basename;
            int num = 1;

            ICollection<string> names = _store.AvailableFontNames;
            while (names.Contains(name))
            {
                name = string.Format(basename, ++num);
            }
            return name;
        }

        /// <summary>
        /// Generates a Blue Prism font from the given system font.
        /// </summary>
        /// <param name="windowsFont">The system font from which a BP font should be
        /// generated</param>
        /// <param name="mon">A progress monitor to which progress should be reported
        /// and which may provide a cancellation request.</param>
        /// <returns>The generated Blue Prism font with an auto-generated name, null
        /// if the generation was cancelled</returns>
        public BPFont GenerateFont(Font windowsFont, clsProgressMonitor mon)
        {
            return GenerateFont(windowsFont, GenerateName(windowsFont), mon);
        }

        /// <summary>
        /// Generates a Blue Prism font from the given system font.
        /// </summary>
        /// <param name="f">The system font from which a BP font should be generated
        /// </param>
        /// <param name="name">The name of the font to generate</param>
        /// <param name="mon">A progress monitor to which progress should be reported
        /// and which may provide a cancellation request.</param>
        /// <returns>The generated Blue Prism font, null if the generation was
        /// cancelled</returns>
        public BPFont GenerateFont(Font f, string name, clsProgressMonitor mon)
        {
            //Generate the alphabet, one character per bitmap
            StringFormat sf = new StringFormat();
            FontData data = new FontData();

            //a-z, A-Z, 0-9 and punctuation
            const char first = (char)33;
            const char last = (char)126;
            int count = 0;
            for (char c = first; c <= last; c++)
            {
                string str = c.ToString();
                AddCharacterToFont(data, f, str, sf);
                // Report progress every 5 characters... no need to overdo it
                if (++count % 5 == 0)
                {
                    mon.FireProgressChange(Convert.ToInt32(
                        (25.0 / (last - first)) * (c - first)),
                        string.Format(Resources.Adding0, c)
                    );
                    if (mon.IsCancelRequested)
                        return null;
                }
            }

            AddCharacterToFont(data, f, "£", sf);

            //Trim the spare white space, above and below each character, whilst
            //respecting the tallest and lowest-hanging characters.
            int fontHeight = 0;
            int minRowsAbove = int.MaxValue;
            int minRowsBelow = int.MaxValue;
            foreach (CharData c in data.Characters)
            {
                minRowsAbove = Math.Min(c.Mask.CountEmptyLines(Direction.Top), minRowsAbove);
                minRowsBelow = Math.Min(c.Mask.CountEmptyLines(Direction.Bottom), minRowsBelow);
                fontHeight = Math.Max(fontHeight, c.Height);
            }
            foreach (CharData c in data.Characters)
            {
                c.Strip(Direction.Top, minRowsAbove);
                c.Strip(Direction.Bottom, minRowsBelow);
            }

            //Set space width
            data.SpaceWidth = GetSpaceWidth(f, sf);

            Bitmap bmp = new Bitmap(200, 200);
            Graphics measuregG = Graphics.FromImage(bmp);

            //Get kerning information for each and every pair
            Dictionary<string, int> KernValues = new Dictionary<string, int>();
            int completedCount = 0;
            int totalPairs = data.Characters.Count * data.Characters.Count;
            char i = first;
            foreach (CharData fChar1 in data.Characters)
            {
                foreach (CharData fChar2 in data.Characters)
                {
                    string val1 = fChar1.Value;
                    string val2 = fChar2.Value;

                    mon.FireProgressChange(Math.Min(99,
                        25 + Convert.ToInt32((75.0 / (last - first)) * (i - first))),
                        string.Format(Resources.CalculatingSpacingBetween0And1, val1, val2));
                    if (mon.IsCancelRequested)
                        return null;

                    //Measure the width of these two characters together
                    string testVal = fChar1 + fChar2;
                    SizeF s = measuregG.MeasureString(testVal, f, new Size(int.MaxValue, int.MaxValue), sf);
                    Bitmap combinedBitMap = new Bitmap(Convert.ToInt32(Math.Ceiling(s.Width)) + 4, Convert.ToInt32(Math.Ceiling(s.Height)) + 2);
                    using (Graphics g = Graphics.FromImage(combinedBitMap))
                    {
                        g.Clear(Color.White);
                        g.DrawString(testVal, f, Brushes.Black, 1, 1, sf);
                    }
                    AutoTrimImageSides(ref combinedBitMap, Color.Black);
                    int CombinedWidth = combinedBitMap.Width;

                    //Get the width of the left char alone
                    testVal = val1;
                    s = measuregG.MeasureString(testVal, f, new Size(int.MaxValue, int.MaxValue), sf);
                    Bitmap LeftBitMap = new Bitmap(Convert.ToInt32(Math.Ceiling(s.Width)) + 4, Convert.ToInt32(Math.Ceiling(s.Height)) + 2);
                    using (Graphics g = Graphics.FromImage(LeftBitMap))
                    {
                        g.Clear(Color.White);
                        g.DrawString(testVal, f, Brushes.Black, 1, 1, sf);
                    }
                    AutoTrimImageSides(ref LeftBitMap, Color.Black);
                    int LeftCharWidth = LeftBitMap.Width;

                    //Get the width of the right char alone
                    testVal = val2;
                    s = measuregG.MeasureString(testVal, f, new Size(int.MaxValue, int.MaxValue), sf);
                    Bitmap RightBitMap = new Bitmap(Convert.ToInt32(Math.Ceiling(s.Width)) + 4, Convert.ToInt32(Math.Ceiling(s.Height)) + 2);
                    using (Graphics g = Graphics.FromImage(RightBitMap))
                    {
                        g.Clear(Color.White);
                        g.DrawString(testVal, f, Brushes.Black, 1, 1, sf);
                    }
                    AutoTrimImageSides(ref RightBitMap, Color.Black);
                    int RightCharWidth = RightBitMap.Width;


                    //The kerning value is usually just the difference in width
                    //between the combined, vs the sum of the parts. However,
                    //where the width remains unchanged, the LH char must be
                    //completely pushed forward into the RH character's space
                    //(in which case there is no saying how far inwards the LH
                    //character is pushed, and therefore how big the kerning value is.
                    //Eg who is to say that in an italic font, the '_ pair may have
                    //the apostrophe centred over the underscore?
                    //Therefore the following more complicated routine is employed
                    //for negatively kerned pairs.
                    int KernValue = CombinedWidth - LeftCharWidth - RightCharWidth;

                    if (-KernValue >= LeftCharWidth)
                    {
                        //We build a single temporary font character as a composite of
                        //the two, at various kerning intervals. Once a full strict
                        //match is achieved then we have found the appropriate kerning
                        //distance
                        Debug.Assert(LeftBitMap.Height == RightBitMap.Height && LeftBitMap.Height == combinedBitMap.Height);
                        CharCanvas Canvas = CharCanvas.FromPixRectByForegroundColour(new clsPixRect(combinedBitMap), 0);
                        for (int PossibleKernValue = KernValue; PossibleKernValue >= -RightBitMap.Width; PossibleKernValue += -1)
                        {
                            bool[,] CompositeData = new bool[combinedBitMap.Width, combinedBitMap.Height];
                            //Copy the RH char, aligning as far right as poss
                            for (int x = 0; x <= RightBitMap.Width - 1; x++)
                            {
                                for (int y = 0; y <= RightBitMap.Height - 1; y++)
                                {
                                    bool RightBMIsBlack = RightBitMap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb();
                                    CompositeData[combinedBitMap.Width - RightBitMap.Width + x, y] = RightBMIsBlack;
                                }
                            }
                            //Copy the LH char, aligning to the RHS, minus width of RH char,
                            //plus the proposed kerning distance.
                            for (int x = 0; x <= LeftBitMap.Width - 1; x++)
                            {
                                for (int y = 0; y <= LeftBitMap.Height - 1; y++)
                                {
                                    bool LeftBMIsBlack = LeftBitMap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb();
                                    int Xcoord = combinedBitMap.Width - LeftBitMap.Width - RightBitMap.Width - PossibleKernValue + x;
                                    CompositeData[Xcoord, y] = CompositeData[Xcoord, y] || LeftBMIsBlack;
                                }
                            }

                            CharData chr = new CharData(".", new Mask(CompositeData));
                            bool nonStrict;
                            if (chr.IsAtPosition(Point.Empty, Canvas, out nonStrict, true, true))
                            {
                                KernValue = PossibleKernValue;
                                break;
                            }
                        }
                    }
                    KernValues.Add(fChar1 + fChar2, KernValue);

                    //Compute the "invasions" within the overlap area. The following
                    //variables record the start/end indices relative to the combined
                    //bitmap
                    int LeftCharStartIndex_X = Math.Max(0, -LeftCharWidth - KernValue);
                    int LeftCharEndIndex_X = LeftCharStartIndex_X + LeftCharWidth - 1;
                    int RightCharStartIndex_X = CombinedWidth - RightCharWidth;
                    int RightCharEndIndex_X = RightCharStartIndex_X + RightCharWidth - 1;

                    PixelState[,] Mask1 = fChar1.StateMask.CopiedValue;
                    PixelState[,] Mask2 = fChar2.StateMask.CopiedValue;

                    //Find out where the 'top' of the characters is, in this large sample
                    //making the assumption that the top is at the same position as when
                    //left hand char is rendered alone
                    int Top = -1;
                    int GlyphHeight = fChar1.Height;
                    CharCanvas canv = CharCanvas.FromPixRectByForegroundColour(new clsPixRect(LeftBitMap), 0);
                    for (int y = 0; y <= LeftBitMap.Height - 1; y++)
                    {
                        bool nonStrict;
                        if (fChar1.IsAtPosition(new Point(0, y), canv, out nonStrict, true, true))
                        {
                            Top = y;
                            break;
                        }
                    }
                    if (Top < 0)
                        throw new InvalidOperationException(string.Format(Resources.FailedToFindTopOfLeftCharacterIn01, val1, val2));

                    for (int x = 0; x <= CombinedWidth - 1; x++)
                    {
                        for (int y = 0; y <= GlyphHeight - 1; y++)
                        {
                            bool InsideLeftChar = x >= LeftCharStartIndex_X && x <= LeftCharEndIndex_X;
                            bool InsideRightChar = x >= RightCharStartIndex_X && x <= RightCharEndIndex_X;
                            if (InsideLeftChar && InsideRightChar)
                            {
                                int LeftCharRelativeX = x - LeftCharStartIndex_X;
                                int RightCharRelativeX = x - RightCharStartIndex_X;
                                bool LeftCharBlack = fChar1.Mask[LeftCharRelativeX, y];
                                bool RightCharBlack = fChar2.Mask[RightCharRelativeX, y];
                                if (RightCharBlack)
                                    Mask1[LeftCharRelativeX, y] = PixelState.NoCheck;
                                if (LeftCharBlack)
                                    Mask2[RightCharRelativeX, y] = PixelState.NoCheck;
                            }
                        }
                    }

                    completedCount += 1;

                    fChar1.StateMask = new PixelStateMask(Mask1);
                    fChar2.StateMask = new PixelStateMask(Mask2);
                }
                i++;
            }
            data.SetKernValues(KernValues);

            mon.FireProgressChange(99, string.Format(Resources.GeneratedBluePrismFont0, name));

            return new BPFont(name, data);
        }

        #endregion

    }

    //this class is part of the python project https://dev.azure.com/bpresearch/OCR/_git/bpocr.git and has been added to BP with some modifications
    public static class SystemFontExtractorOcrPlus
        {
            private static char[] CHARACTERS = new char[174];

            static SystemFontExtractorOcrPlus()
            {
                // selected charset
                int i = 0;
                for (int c = 48; c <= 57; c++)
                    CHARACTERS[i++] = (char)c;
                for (int c = 65; c <= 90; c++)
                    CHARACTERS[i++] = (char)c;
                for (int c = 97; c <= 122; c++)
                    CHARACTERS[i++] = (char)c;
                // basic symbols
                for (int c = 33; c <= 47; c++)
                    CHARACTERS[i++] = (char)c;
                for (int c = 58; c <= 64; c++)
                    CHARACTERS[i++] = (char)c;
                for (int c = 91; c <= 96; c++)
                    CHARACTERS[i++] = (char)c;
                for (int c = 123; c <= 126; c++)
                    CHARACTERS[i++] = (char)c;
            Debug.Assert(i == 94);
            // extended symbols
            foreach (char c in "¡¢£¥§©®°±²³µ¼½¾¿")
                CHARACTERS[i++] = c;
            // accentuated characters etc
            Debug.Assert(((char)192).ToString() == "À");
            Debug.Assert(((char)255).ToString() == "ÿ");
            for (int c = 192; c <= 255; c++)
                CHARACTERS[i++] = (char)c;
            Debug.Assert(i == CHARACTERS.Length);
        }

            private static bool USE_GDI = false;
            private static int X = 2;
            private static int Y = 3;

            private static void DrawText(Graphics g, string text, Font font)
            {
                DrawText(g, text, font, false, USE_GDI);
            }

            private static void DrawText(Graphics g, string text, Font font, bool inverted, bool useGDI)
            {
                Color foreground = inverted ? Color.White : Color.Black;
                Color background = inverted ? Color.Black : Color.White;
                g.Clear(background);
                if (useGDI)
                {
                    // Can potentially be optimized a lot by using native draw methods and grouping draw operations?
                    // https://theartofdev.com/2013/08/12/the-wonders-of-text-rendering-and-gdi/
                    // https://theartofdev.com/2013/08/12/using-native-gdi-for-text-rendering-in-c/
                    TextRenderer.DrawText(g, text, font, new Point(X, Y), foreground, background, TextFormatFlags.NoPrefix);
                }
                else
                {
                    StringFormat sf = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoFontFallback | StringFormatFlags.NoClip);
                    g.DrawString(text, font, new SolidBrush(foreground), X, Y, sf);
                }
            }

            private static Size MeasureText(Graphics g, string text, Font font)
            {
                return MeasureText(g, text, font, USE_GDI);
            }

            private static Size MeasureText(Graphics g, string text, Font font, bool useGDI)
            {
                if (useGDI)
                {
                    // Can be optimized similarly to DrawText() above?
                    return TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
                }
                else
                {
                    StringFormat sf = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoFontFallback | StringFormatFlags.NoClip);
                    SizeF size = g.MeasureString(text, font, new Size(int.MaxValue, int.MaxValue), sf);
                    return new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
                }
            }

            private static Bitmap TrimBitmap(Bitmap bitmap)
            {
                // Horizontal position of first black pixel, plus check first and last row is white
                int first = -1;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Debug.Assert(bitmap.GetPixel(x, 0).ToArgb() == Color.White.ToArgb());
                    Debug.Assert(bitmap.GetPixel(x, bitmap.Height - 1).ToArgb() == Color.White.ToArgb());
                    if (first == -1)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            if (bitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                            {
                                first = x;
                                break;
                            }
                        }
                    }
                }
                Debug.Assert(first >= 1);

                // Horizontal position of last black pixel
                int last = -1;
                for (int x = bitmap.Width - 1; x >= 0; x += -1)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        if (bitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                        {
                            last = x;
                            break;
                        }
                    }
                    if (last != -1)
                        break;
                }
                Debug.Assert(last <= bitmap.Width - 2);

                // Trim bitmap horizontally, with additional left and right margin
                int width = last - first + 3;
                Bitmap result = bitmap.Clone(new Rectangle(first - 1, 0, width, bitmap.Height), bitmap.PixelFormat);
                bitmap.Dispose();

                return result;
            }

            private static void AddCharacter(FontData bpFont, Font font, Font underline, Font strikeout, int fontHeight, string text, Graphics measure)
            {
                // Text size
                Size size = MeasureText(measure, text, font);

                // Character bitmap
                Debug.Assert(size.Height == fontHeight);
                Bitmap bitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                    DrawText(g, text, font);
                }

                // Check underline version, and initialize underline y-value
                using (Bitmap other = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y))
                {
                    using (Graphics g = Graphics.FromImage(other))
                    {
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                        DrawText(g, text, underline);
                    }
                    bool initialize = (bpFont.Underline.Count == 0);
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        if (bpFont.Underline.Contains(y))
                        {
                            bool? anyBlack = null;  // Any extra pixel black?
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                if (other.GetPixel(x, y).ToArgb() != bitmap.GetPixel(x, y).ToArgb())
                                {
                                    // If mismatch, regular white and underline black
                                    Debug.Assert(bitmap.GetPixel(x, y).ToArgb() == Color.White.ToArgb());
                                    Debug.Assert(other.GetPixel(x, y).ToArgb() == Color.Black.ToArgb());
                                    anyBlack = true;  // Can overwrite false to true
                                }
                                else if (!anyBlack.HasValue && other.GetPixel(x, y).ToArgb() == Color.White.ToArgb() && x >= 1 && x < bitmap.Width - 1)
                                {
                                    anyBlack = false;  // Cannot overwrite true to false
                                }
                            }
                        }
                        else
                        {
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                if (other.GetPixel(x, y).ToArgb() != bitmap.GetPixel(x, y).ToArgb())
                                {
                                    // Initialize underline
                                    Debug.Assert(initialize && bpFont.Underline.Count <= 1);
                                    bpFont.Underline.Add(y);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Check strikeout version, and initialize strikeout y-value (same as underline)
                using (Bitmap other = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y))
                {
                    using (Graphics g = Graphics.FromImage(other))
                    {
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                        DrawText(g, text, strikeout);
                    }
                    bool initialize = (bpFont.Strikeout.Count == 0);
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        if (bpFont.Strikeout.Contains(y))
                        {
                            bool? anyBlack = null;  // Any extra pixel black?
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                if (other.GetPixel(x, y).ToArgb() != bitmap.GetPixel(x, y).ToArgb())
                                {
                                    // If mismatch, regular white and strikeout black
                                    Debug.Assert(bitmap.GetPixel(x, y).ToArgb() == Color.White.ToArgb());
                                    Debug.Assert(other.GetPixel(x, y).ToArgb() == Color.Black.ToArgb());
                                    anyBlack = true;  // Can overwrite false to true
                                }
                                else if (!anyBlack.HasValue && other.GetPixel(x, y).ToArgb() == Color.White.ToArgb() && x >= 1 && x < bitmap.Width - 1)
                                {
                                    anyBlack = false;  // Cannot overwrite true to false
                                }
                            }
                        }
                        else
                        {
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                if (other.GetPixel(x, y).ToArgb() != bitmap.GetPixel(x, y).ToArgb())
                                {
                                    // Initialize strikeout
                                    Debug.Assert(initialize && bpFont.Strikeout.Count <= 1);
                                    bpFont.Strikeout.Add(y);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Trim character bitmap horizontally
                bitmap = TrimBitmap(bitmap);

                // Add character to BP font
                CharData bpChar = new CharData(text, bitmap);
                bpFont.AddCharacter(bpChar, false);
                bitmap.Dispose();
            }

            private static int GetSpaceWidth(Font font, Graphics measure)
            {
                int spaceWidth = -1;
                float sw = -1.0F;
                foreach (char c in CHARACTERS)
                {
                    // Measure strings
                    string charText = c.ToString();
                    string spaceText = charText + " " + charText;
                    string dspaceText = charText + "  " + charText;

                    if (USE_GDI)
                    {
                        Size spaceSize = MeasureText(measure, spaceText, font);
                        Size dspaceSize = MeasureText(measure, dspaceText, font);

                        // Initialize space width or check consistency
                        if (spaceWidth == -1)
                        {
                            spaceWidth = dspaceSize.Width - spaceSize.Width;
                            Debug.Assert(spaceWidth > 0);
                        }
                        else
                        {
                            Debug.Assert(dspaceSize.Width - spaceSize.Width == spaceWidth);
                        }

                    }
                    else
                    {
                        StringFormat sf = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoFontFallback | StringFormatFlags.NoClip);
                        SizeF spaceSize = measure.MeasureString(spaceText, font, new Size(int.MaxValue, int.MaxValue), sf);
                        SizeF dspaceSize = measure.MeasureString(dspaceText, font, new Size(int.MaxValue, int.MaxValue), sf);

                        // Initialize space width or check consistency
                        if (spaceWidth == -1)
                        {
                            sw = dspaceSize.Width - spaceSize.Width;
                            Debug.Assert(sw > 0.0F);
                            spaceWidth = (int)Math.Round(sw);
                        }
                        else
                        {
                            Debug.Assert(Math.Abs(dspaceSize.Width - spaceSize.Width - sw) < 0.001F);
                        }
                    }
                }

                return spaceWidth;
            }

            public static BPFont GenerateFont(Font font, string name, clsProgressMonitor mon)
            {
                FontData bpFont = new FontData();

                // Underline and strikeout variants
                Font underline = new Font(font.FontFamily, font.Size, font.Style | FontStyle.Underline);
                Font strikeout = new Font(font.FontFamily, font.Size, font.Style | FontStyle.Strikeout);

                // Graphics object used for measuring
                Bitmap measureBitmap = new Bitmap(1, 1);
                Graphics measure = Graphics.FromImage(measureBitmap);
                measure.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                // Measure font height
                Size size = MeasureText(measure, new string(CHARACTERS), font);
                int fontHeight = size.Height;

                // Add characters to FontData object
                foreach (char character in CHARACTERS)
                {
                    AddCharacter(bpFont, font, underline, strikeout, fontHeight, character.ToString(), measure);
                }

                // Margin
                int margin = 1;

                //Trim the spare white space, above and below each character, whilst
                //respecting the tallest and lowest-hanging characters.
                int minRowsAbove = int.MaxValue;
                int minRowsBelow = int.MaxValue;
                fontHeight = 0;
                foreach (CharData c in bpFont.Characters)
                {
                    minRowsAbove = Math.Min(c.Mask.CountEmptyLines(Direction.Top), minRowsAbove);
                    minRowsBelow = Math.Min(c.Mask.CountEmptyLines(Direction.Bottom), minRowsBelow);
                    fontHeight = Math.Max(fontHeight, c.Height);
                }
                Debug.Assert(minRowsAbove >= 1);
                Debug.Assert(minRowsBelow >= 1);
                // Additional top and bottom margin
                minRowsAbove -= 1;
                minRowsBelow -= 1;
                if (bpFont.Underline.Count > 0)
                {
                    Debug.Assert(bpFont.Underline.Max() < minRowsAbove + fontHeight && bpFont.Strikeout.Max() < bpFont.Underline.Min());
                    bpFont.Underline = new HashSet<int>(from y in bpFont.Underline select y - minRowsAbove);
                    bpFont.Strikeout = new HashSet<int>(from y in bpFont.Strikeout select y - minRowsAbove);
                }
                foreach (CharData c in bpFont.Characters)
                {
                    Debug.Assert(c.Mask.CountEmptyLines(Direction.Left) == margin);
                    Debug.Assert(c.Mask.CountEmptyLines(Direction.Right) == margin);
                    c.Strip(Direction.Top, minRowsAbove);
                    c.Strip(Direction.Bottom, minRowsBelow);
                    Debug.Assert(c.Mask.CountEmptyLines(Direction.Top) >= margin);
                    Debug.Assert(c.Mask.CountEmptyLines(Direction.Bottom) >= margin);
                }

                // Set space width
                bpFont.SpaceWidth = GetSpaceWidth(font, measure);

                // Get kerning information for each and every pair
                Dictionary<string, int> kerning = new Dictionary<string, int>();
                Dictionary<string, int> kerningSpace = new Dictionary<string, int>();

                bool failure = false;
                double totalFontChars = bpFont.Characters.Count;
                double progressChangePercent = 0;
                int progressChange = 0;
                foreach (CharData char1 in bpFont.Characters)
                {
                    ++progressChange;
                    progressChangePercent = progressChange * (100 / totalFontChars);
                    mon.FireProgressChange(Convert.ToInt32(progressChangePercent), string.Format(Resources.GenerateABluePrismOCRPlusFont, name));
                    if (mon.IsCancelRequested)
                        return null;

                    // Measure the width of the left char alone
                    string text = char1.Value;
                    size = MeasureText(measure, text, font);
                    Bitmap leftBitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y);
                    using (Graphics g = Graphics.FromImage(leftBitmap))
                    {
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                        DrawText(g, text, font);
                    }
                    leftBitmap = TrimBitmap(leftBitmap);

                    foreach (CharData char2 in bpFont.Characters)
                    {
                        // Measure the width of the right char alone
                        text = char2.Value;
                        size = MeasureText(measure, text, font);
                        Bitmap rightBitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y);
                        using (Graphics g = Graphics.FromImage(rightBitmap))
                        {
                            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                            DrawText(g, text, font);
                        }
                        rightBitmap = TrimBitmap(rightBitmap);

                        // Measure the width of these two characters together
                        text = char1 + char2;
                        size = MeasureText(measure, text, font);
                        Bitmap combinedBitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y);
                        using (Graphics g = Graphics.FromImage(combinedBitmap))
                        {
                            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                            DrawText(g, text, font);
                        }
                        combinedBitmap = TrimBitmap(combinedBitmap);

                        // Check first and last column not empty
                        bool anyStart = false, anyEnd = false;
                        for (int y = 0; y < combinedBitmap.Height; y++)
                        {
                            if (combinedBitmap.GetPixel(margin, y).ToArgb() == Color.Black.ToArgb())
                                anyStart = true;
                            if (combinedBitmap.GetPixel(combinedBitmap.Width - 1 - margin, y).ToArgb() == Color.Black.ToArgb())
                                anyEnd = true;
                            if (anyStart && anyEnd)
                                break;
                        }
                        Debug.Assert(anyStart && anyEnd);

                        // Check vertical shift
                        int? leftOffset = null;
                        for (int y = 0; y < leftBitmap.Height; y++)
                        {
                            for (int x = 0; x < leftBitmap.Width; x++)
                            {
                                if (!leftOffset.HasValue && leftBitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                                {
                                    leftOffset = y;
                                    break;
                                }
                            }
                            if (leftOffset.HasValue)
                                break;
                        }
                        int? rightOffset = null;
                        for (int y = 0; y < rightBitmap.Height; y++)
                        {
                            for (int x = 0; x < rightBitmap.Width; x++)
                            {
                                if (!rightOffset.HasValue && rightBitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                                {
                                    rightOffset = y;
                                    break;
                                }
                            }
                            if (rightOffset.HasValue)
                                break;
                        }
                        Debug.Assert(leftOffset.HasValue && rightOffset.HasValue);
                        leftOffset = leftOffset.Value - margin;
                        rightOffset = rightOffset.Value - margin;
                        Debug.Assert(leftOffset.Value >= 0 && rightOffset.Value >= 0);

                        //The kerning value is usually just the difference in width
                        //between the combined, vs the sum of the parts. However,
                        //where the width remains unchanged, the LH char must be
                        //completely pushed forward into the RH character's space
                        //(in which case there is no saying how far inwards the LH
                        //character is pushed, and therefore how big the kerning value is.
                        //Eg who is to say that in an italic font, the '_ pair may have
                        //the apostrophe centred over the underscore?
                        //Therefore the following more complicated routine is employed
                        //for negatively kerned pairs.

                        // Kerning
                        int leftStart = -1, leftEnd = -1, rightStart = -1, rightEnd = -1, kernValue = -1, frm = -1, to = -1;
                        PixelState[,] mask1 = char1.StateMask.CopiedValue;
                        PixelState[,] mask2 = char2.StateMask.CopiedValue;

                        // Range of potential kerning values
                        int maxCharWidth = Math.Max(leftBitmap.Width, rightBitmap.Width);
                        if (combinedBitmap.Width > maxCharWidth)
                        {
                            frm = to = combinedBitmap.Width - leftBitmap.Width - rightBitmap.Width;
                        }
                        else
                        {
                            frm = -Math.Min(leftBitmap.Width, rightBitmap.Width);
                            to = -maxCharWidth;
                        }
                        Debug.Assert(frm >= to);
                        bool mismatch = true;
                        int corrections = 0;
                        for (kernValue = frm; kernValue >= to; kernValue--)
                        {

                            // Character left/right start/end indices
                            if (rightBitmap.Width == combinedBitmap.Width)
                            {
                                rightStart = 0;
                                rightEnd = combinedBitmap.Width;
                                leftEnd = rightStart - kernValue;
                                Debug.Assert(leftEnd <= combinedBitmap.Width);
                                leftStart = leftEnd - leftBitmap.Width;
                                Debug.Assert(leftStart >= 0);
                            }
                            else
                            {
                                leftStart = 0;
                                leftEnd = Math.Min(leftBitmap.Width, combinedBitmap.Width);
                                rightStart = leftEnd + kernValue;
                                Debug.Assert(rightStart >= 0);
                                rightEnd = rightStart + rightBitmap.Width;
                                Debug.Assert(rightEnd <= combinedBitmap.Width);
                            }

                            // Check for match between combined and left/right
                            mismatch = false;
                            for (int x = 0; x < combinedBitmap.Width; x++)
                            {
                                for (int y = 0; y < combinedBitmap.Height; y++)
                                {
                                    if (combinedBitmap.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                                    {
                                        if (x >= leftStart && x < leftEnd && leftBitmap.GetPixel(x - leftStart, y).ToArgb() == Color.Black.ToArgb())
                                        {
                                            // Match: combined true and left true
                                            Debug.Assert(y >= leftOffset.Value + margin && y < leftOffset.Value + char1.Height - margin);
                                            continue;
                                        }
                                        if (x >= rightStart && x < rightEnd && rightBitmap.GetPixel(x - rightStart, y).ToArgb() == Color.Black.ToArgb())
                                        {
                                            // Match: combined true and right true
                                            Debug.Assert(y >= rightOffset.Value + margin && y < rightOffset.Value + char2.Height - margin);
                                            continue;
                                        }
                                        // Mismatch: combined true and left/right not true
                                        mismatch = true;
                                        if (frm == to)
                                        {
                                            // If only kerning candidate, modify statemasks (at least one needs to be changed)
                                            if (x >= leftStart && x < leftEnd && y >= leftOffset.Value && y < leftOffset.Value + char1.Height)
                                            {
                                                mask1[x - leftStart, y - leftOffset.Value] = PixelState.NoCheck;
                                                mismatch = false;
                                            }
                                            if (x >= rightStart && x < rightEnd && y >= rightOffset.Value && y < rightOffset.Value + char2.Height)
                                            {
                                                mask2[x - rightStart, y - rightOffset.Value] = PixelState.NoCheck;
                                                mismatch = false;
                                            }
                                            corrections++;
                                        }
                                        if (mismatch)
                                            break;
                                    }
                                    else
                                    {
                                        bool correctionDone = false;
                                        if (x >= leftStart && x < leftEnd && leftBitmap.GetPixel(x - leftStart, y).ToArgb() != Color.White.ToArgb())
                                        {
                                            // Mismatch: combined false and left not false
                                            if (frm == to && y >= leftOffset.Value && y < leftOffset.Value + char1.Height)
                                            {
                                                // If only kerning candidate, modify statemask
                                                mask1[x - leftStart, y - leftOffset.Value] = PixelState.NoCheck;
                                                corrections++;
                                                correctionDone = true;
                                            }
                                            else
                                            {
                                                mismatch = true;
                                                break;
                                            }
                                        }
                                        if (x >= rightStart && x < rightEnd && rightBitmap.GetPixel(x - rightStart, y).ToArgb() != Color.White.ToArgb())
                                        {
                                            // Mismatch: combined false and right not false
                                            if (frm == to && y >= rightOffset.Value && y < rightOffset.Value + char2.Height)
                                            {
                                                // If only kerning candidate, modify statemask
                                                mask2[x - rightStart, y - rightOffset.Value] = PixelState.NoCheck;
                                                if (!correctionDone)
                                                    corrections++;
                                            }
                                            else
                                            {
                                                mismatch = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (mismatch)
                                    break;
                            }

                            // If no mismatch, kerning value found
                            if (!mismatch)
                                break;
                        }

                        // If no kerning value found, failure
                        if (mismatch || corrections > Math.Max(char1.Height, char2.Height) / 3)
                        {
                            kerning.Add(char1 + char2, -1000);

                        }
                        else
                        {
                            kerning.Add(char1 + char2, kernValue);

                            // Statemask
                            //Compute the "invasions" within the overlap area. The following
                            //variables record the start/end indices relative to the combined
                            //bitmap
                            for (int x = 0; x < combinedBitmap.Width; x++)
                            {
                                if ((x >= leftStart && x < leftEnd) && (x >= rightStart && x < rightEnd))
                                {
                                    for (int y = 0; y < char1.Height; y++)
                                    {
                                        if (char2.Mask[x - rightStart, y])
                                            mask1[x - leftStart, y] = PixelState.NoCheck;
                                        if (char1.Mask[x - leftStart, y])
                                            mask2[x - rightStart, y] = PixelState.NoCheck;
                                    }
                                }
                            }

                            // Statemask
                            char1.StateMask = new PixelStateMask(mask1);
                            char2.StateMask = new PixelStateMask(mask2);
                        }

                        combinedBitmap.Dispose();

                        // Space kerning

                        // Measure the combined width with space
                        int spaceWidth;
                        text = char1.Value + " " + char2.Value;
                        size = MeasureText(measure, text, font);
                        using (Bitmap bitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                                DrawText(g, text, font);
                            }
                            spaceWidth = TrimBitmap(bitmap).Width;
                        }

                        // Measure the combined width with double space
                        text = char1.Value + "  " + char2.Value;
                        size = MeasureText(measure, text, font);
                        using (Bitmap bitmap = new Bitmap(size.Width + 2 * X + 1, fontHeight + 2 * Y))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                                DrawText(g, text, font);
                            }
                        }

                        // Measure kerning for combined with space
                        int kernValueSpace = spaceWidth - leftBitmap.Width - rightBitmap.Width + 2 - bpFont.SpaceWidth;
                        kerningSpace.Add(char1 + char2, kernValueSpace);

                        rightBitmap.Dispose();
                    }

                    leftBitmap.Dispose();

                    if (failure)
                        break;
                }

                // Set kerning values
                bpFont.SetKernValues(kerning);
                bpFont.SetKernValuesSpace(kerningSpace);

                measure.Dispose();
                measureBitmap.Dispose();

                return new BPFont(name, bpFont);
            }
        }


}
