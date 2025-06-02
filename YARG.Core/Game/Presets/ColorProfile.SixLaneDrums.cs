using System.Drawing;
using System.IO;
using YARG.Core.Extensions;
using YARG.Core.Utility;

namespace YARG.Core.Game
{
    public partial class ColorProfile
    {
        public class SixLaneDrumsColors : IFretColorProvider, IBinarySerializable
        {
            #region Frets

            public Color KickFret   = DefaultOrange;
            public Color RedFret    = DefaultRed;
            public Color SilverFret = DefaultSilver;
            public Color YellowFret = DefaultYellow;
            public Color BlueFret   = DefaultBlue;
            public Color GreenFret  = DefaultGreen;
            public Color PurpleFret = DefaultPurple;

            /// <summary>
            /// Gets the fret color for a specific note index.
            /// 0 = kick note, 1 = red, 6 = purple.
            /// </summary>
            public Color GetFretColor(int index)
            {
                return index switch
                {
                    0 => KickFret,
                    1 => RedFret,
                    2 => SilverFret,
                    3 => YellowFret,
                    4 => BlueFret,
                    5 => GreenFret,
                    6 => PurpleFret,
                    _ => default
                };
            }

            public Color KickFretInner   = DefaultOrange;
            public Color RedFretInner    = DefaultRed;
            public Color SilverFretInner = DefaultSilver;
            public Color YellowFretInner = DefaultYellow;
            public Color BlueFretInner   = DefaultBlue;
            public Color GreenFretInner  = DefaultGreen;
            public Color PurpleFretInner = DefaultPurple;

            /// <summary>
            /// Gets the inner fret color for a specific note index.
            /// 0 = kick note, 1 = red, 6 = purple.
            /// </summary>
            public Color GetFretInnerColor(int index)
            {
                return index switch
                {
                    0 => KickFretInner,
                    1 => RedFretInner,
                    2 => SilverFretInner,
                    3 => YellowFretInner,
                    4 => BlueFretInner,
                    5 => GreenFretInner,
                    6 => PurpleFretInner,
                    _ => default
                };
            }

            public Color KickParticles   = DefaultOrange;
            public Color RedParticles    = DefaultRed;
            public Color SilverParticles = DefaultSilver;
            public Color YellowParticles = DefaultYellow;
            public Color BlueParticles   = DefaultBlue;
            public Color GreenParticles  = DefaultGreen;
            public Color PurpleParticles = DefaultPurple;

            /// <summary>
            /// Gets the particle color for a specific note index.
            /// 0 = kick note, 1 = red, 6 = purple.
            /// </summary>
            public Color GetParticleColor(int index)
            {
                return index switch
                {
                    0 => KickParticles,
                    1 => RedParticles,
                    2 => SilverParticles,
                    3 => YellowParticles,
                    4 => BlueParticles,
                    5 => GreenParticles,
                    6 => PurpleParticles,
                    _ => default
                };
            }

            #endregion

            #region Notes

            public Color KickNote = DefaultOrange;

            public Color RedNote    = DefaultRed;
            public Color SilverNote = DefaultSilver;
            public Color YellowNote = DefaultYellow;
            public Color BlueNote   = DefaultBlue;
            public Color GreenNote  = DefaultGreen;
            public Color PurpleNote = DefaultPurple;

            /// <summary>
            /// Gets the note color for a specific note index.
            /// 0 = kick note, 1 = red drum, 5 = green drum.
            /// </summary>
            public Color GetNoteColor(int index)
            {
                return index switch
                {
                    0 => KickNote,

                    1 => RedNote,
                    2 => SilverNote,
                    3 => YellowNote,
                    4 => BlueNote,
                    5 => GreenNote,
                    6 => PurpleNote,
                    _ => default
                };
            }

            public Color KickStarpower = DefaultStarpower;

            public Color RedStarpower    = DefaultStarpower;
            public Color SilverStarpower = DefaultStarpower;
            public Color YellowStarpower = DefaultStarpower;
            public Color BlueStarpower   = DefaultStarpower;
            public Color GreenStarpower  = DefaultStarpower;
            public Color PurpleStarpower = DefaultStarpower;

            /// <summary>
            /// Gets the Star Power note color for a specific note index.
            /// 0 = kick note, 1 = red drum, 5 = green drum.
            /// </summary>
            public Color GetNoteStarPowerColor(int index)
            {
                return index switch
                {
                    0 => KickStarpower,

                    1 => RedStarpower,
                    2 => SilverStarpower,
                    3 => YellowStarpower,
                    4 => BlueStarpower,
                    5 => GreenStarpower,
                    6 => PurpleStarpower,
                    _ => default
                };
            }

            public Color ActivationNote = DefaultPurple; // TODO

            #endregion

            #region Serialization

            public SixLaneDrumsColors Copy()
            {
                // Kinda yucky, but it's easier to maintain
                return (SixLaneDrumsColors) MemberwiseClone();
            }

            public void Serialize(BinaryWriter writer)
            {
                writer.Write(KickFret);
                writer.Write(RedFret);
                writer.Write(SilverFret);
                writer.Write(YellowFret);
                writer.Write(BlueFret);
                writer.Write(GreenFret);
                writer.Write(PurpleFret);

                writer.Write(KickFretInner);
                writer.Write(RedFretInner);
                writer.Write(SilverFretInner);
                writer.Write(YellowFretInner);
                writer.Write(BlueFretInner);
                writer.Write(GreenFretInner);
                writer.Write(PurpleFretInner);

                writer.Write(KickParticles);
                writer.Write(RedParticles);
                writer.Write(SilverParticles);
                writer.Write(YellowParticles);
                writer.Write(BlueParticles);
                writer.Write(GreenParticles);
                writer.Write(PurpleParticles);

                writer.Write(KickNote);
                writer.Write(RedNote);
                writer.Write(SilverNote);
                writer.Write(YellowNote);
                writer.Write(BlueNote);
                writer.Write(GreenNote);
                writer.Write(PurpleNote);

                writer.Write(KickStarpower);
                writer.Write(RedStarpower);
                writer.Write(SilverStarpower);
                writer.Write(YellowStarpower);
                writer.Write(BlueStarpower);
                writer.Write(GreenStarpower);
                writer.Write(PurpleStarpower);

                writer.Write(ActivationNote);
            }

            public void Deserialize(BinaryReader reader, int version = 0)
            {
                KickFret = reader.ReadColor();
                RedFret = reader.ReadColor();
                YellowFret = reader.ReadColor();
                SilverFret = reader.ReadColor();
                BlueFret = reader.ReadColor();
                GreenFret = reader.ReadColor();
                PurpleFret = reader.ReadColor();

                KickFretInner = reader.ReadColor();
                RedFretInner = reader.ReadColor();
                SilverFretInner = reader.ReadColor();
                YellowFretInner = reader.ReadColor();
                BlueFretInner = reader.ReadColor();
                GreenFretInner = reader.ReadColor();
                PurpleFretInner = reader.ReadColor();

                KickParticles = reader.ReadColor();
                RedParticles = reader.ReadColor();
                SilverParticles = reader.ReadColor();
                YellowParticles = reader.ReadColor();
                BlueParticles = reader.ReadColor();
                GreenParticles = reader.ReadColor();
                PurpleParticles = reader.ReadColor();

                KickNote = reader.ReadColor();
                RedNote = reader.ReadColor();
                SilverNote = reader.ReadColor();
                YellowNote = reader.ReadColor();
                BlueNote = reader.ReadColor();
                GreenNote = reader.ReadColor();
                PurpleNote = reader.ReadColor();

                KickStarpower = reader.ReadColor();
                RedStarpower = reader.ReadColor();
                SilverStarpower = reader.ReadColor();
                YellowStarpower = reader.ReadColor();
                BlueStarpower = reader.ReadColor();
                GreenStarpower = reader.ReadColor();
                PurpleStarpower = reader.ReadColor();

                ActivationNote = reader.ReadColor();
            }

            #endregion
        }
    }
}