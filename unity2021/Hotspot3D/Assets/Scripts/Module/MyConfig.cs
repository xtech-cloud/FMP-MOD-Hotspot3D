
using System.Xml.Serialization;

namespace XTC.FMP.MOD.Hotspot3D.LIB.Unity
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class MyConfig : MyConfigBase
    {
        public class Position
        {
            [XmlAttribute("x")]
            public float x { get; set; } = 0;
            [XmlAttribute("y")]
            public float y { get; set; } = 0;
            [XmlAttribute("z")]
            public float z { get; set; } = 0;
        }

        public class Rotation
        {
            [XmlAttribute("x")]
            public float x { get; set; } = 0;
            [XmlAttribute("y")]
            public float y { get; set; } = 0;
            [XmlAttribute("z")]
            public float z { get; set; } = 0;
        }

        public class DebugBox
        {
            [XmlAttribute("visible")]
            public bool visible { get; set; } = false;
            [XmlAttribute("color")]
            public string color { get; set; } = "#FFFF0088";
        }

        public class Hotspot
        {
            [XmlAttribute("key")]
            public string key { get; set; } = "";
            [XmlElement("DebugBox")]
            public DebugBox debugBox { get; set; } = new DebugBox();
            [XmlArray("OnSubjects"), XmlArrayItem("Subject")]
            public Subject[] onSubjectS = new Subject[0];

            [XmlArray("OffSubjects"), XmlArrayItem("Subject")]
            public Subject[] offSubjectS = new Subject[0];
        }

        public class Axis
        {
            [XmlAttribute("rangeMin")]
            public float rangeMin { get; set; } = 0;
            [XmlAttribute("rangeMax")]
            public float rangeMax { get; set; } = 0;
            [XmlAttribute("invert")]
            public bool invert { get; set; } = false;
        }
        public class SpaceGrid
        {
            [XmlElement("Position")]
            public Position position { get; set; } = new Position();
        }

        public class RenderCamera
        {
            [XmlElement("Position")]
            public Position position { get; set; } = new Position();
            [XmlElement("Rotation")]
            public Rotation rotation { get; set; } = new Rotation();
        }

        public class Style
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";

            [XmlElement("SpaceGrid")]
            public SpaceGrid spaceGrid { get; set; } = new SpaceGrid();
            [XmlElement("RenderCamera")]
            public RenderCamera renderCamera { get; set; } = new RenderCamera();
            [XmlElement("PitchAxis")]
            public Axis pitchAxis { get; set; } = new Axis();
            [XmlElement("YawAxis")]
            public Axis yawAxis { get; set; } = new Axis();
            [XmlElement("Hotspot")]
            public Hotspot hotspot { get; set; } = new Hotspot();
        }


        [XmlArray("Styles"), XmlArrayItem("Style")]
        public Style[] styles { get; set; } = new Style[0];
    }
}

