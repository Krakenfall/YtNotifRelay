using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace YtNotifRelay
{
    [XmlRoot("feed", Namespace = "http://www.w3.org/2005/Atom")]
    public class YouTubeFeed
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("updated")]
        public DateTimeOffset Updated { get; set; }

        [XmlElement("entry")]
        public List<YouTubeEntry> Entries { get; set; }
    }

    public class YouTubeEntry
    {
        [XmlElement("id", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string Id { get; set; }

        [XmlElement("videoId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string VideoId { get; set; }

        [XmlElement("channelId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string ChannelId { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("published")]
        public DateTimeOffset Published { get; set; }

        [XmlElement("updated")]
        public DateTimeOffset Updated { get; set; }

        [XmlElement("author")]
        public YouTubeAuthor Author { get; set; }
    }

    public class YouTubeAuthor
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("uri")]
        public string Uri { get; set; }
    }

}
