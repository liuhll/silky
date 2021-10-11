namespace Silky.Rpc.Routing.Template
{
    public class TemplateSegment
    {
        public TemplateSegment(SegmentType segmentType, string value)
        {
            SegmentType = segmentType;
            Value = value;
        }

        public SegmentType SegmentType { get; }

        public string Value { get; set; }
        public bool IsParameter => SegmentType == SegmentType.Path;
    }
}