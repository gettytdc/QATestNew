using System.Drawing;

namespace BluePrism.DataPipeline.UI
{
    public static class DataPipelineOutputConfigUISettings
    {
        public static int LargeFieldWidth => 480;
        public static int SmallFieldWidth => 240;
        public static Color LightBlue => Color.FromArgb(208, 238, 255);

        public static Font StandardFont => new Font(new FontFamily("Segoe UI"), (float)8.25 );
        public static Font BoldFont => new Font(new FontFamily("Segoe UI"), (float)8.25, FontStyle.Bold );
        public static Font HeaderFont => new Font(new FontFamily("Segoe UI"), (float)14.25 );

    }
}
