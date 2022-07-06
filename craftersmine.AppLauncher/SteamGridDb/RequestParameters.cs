using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace craftersmine.AppLauncher.SteamGridDb
{
    public static class RequestParameters
    {
        public static class Platforms
        {
            public const string Steam = "steam";
            public const string Origin = "origin";
            public const string EpicGamesStore = "egs";
            public const string BattleNet = "bnet";
            public const string UbisoftConnect = "uplay";
            public const string Flashpoint = "flashpoint";
            public const string EShop = "eshop";
        }

        public static class Grids
        {
            public static class Dimensions
            {
                public const string W600H900 = "600x900";
                public const string W460H215 = "460x215";
                public const string W920H430 = "920x430";
                public const string W342H482 = "342x482";
                public const string W660H930 = "660x930";
                public const string W512H512 = "512x512";
                public const string W1024H1024 = "1024x1024";
            }

            public static class Styles
            {
                public const string Alternate = "alternate";
                public const string Blurred = "blurred";
                public const string WhiteLogo = "white_logo";
                public const string Material = "material";
                public const string NoLogo = "no_logo";
            }

            public static class Mimes
            {
                public const string Png = "image/png";
                public const string Jpeg = "image/jpeg";
                public const string Webp = "image/webp";
            }

            public static class Types
            {
                public const string Static = "static";
                public const string Animated = "animated";
            }


        }
    }
}
