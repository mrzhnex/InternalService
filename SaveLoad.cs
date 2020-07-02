using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace InternalService
{
    public static class SaveLoad
    {     
        public static readonly XmlSerializer ReportersFormatter = new XmlSerializer(typeof(List<ulong>));
        public static readonly XmlSerializer ReportIdFormatter = new XmlSerializer(typeof(ulong));



        public static void LoadReportId()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Info.GetDataPath(), Info.ReportIdFileName), FileMode.Open))
                {
                    Session.ReportId = (ulong)ReportIdFormatter.Deserialize(fs);
                }
            }
            catch (System.Exception)
            {

            }
        }

        public static void SaveReportId()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Info.GetDataPath(), Info.ReportIdFileName), FileMode.Open))
                {
                    ReportIdFormatter.Serialize(fs, Session.ReportId);
                }
            }
            catch (System.Exception)
            {

            }
        }

        public static void LoadReporters()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Info.GetDataPath(), Info.ReportersFileName), FileMode.Open))
                {
                    Info.Reporters = (List<ulong>)ReportersFormatter.Deserialize(fs);
                }
            }
            catch (System.Exception)
            {

            }
        }

        public static void SaveReporters()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Info.GetDataPath(), Info.ReportersFileName), FileMode.Open))
                {
                    ReportersFormatter.Serialize(fs, Info.Reporters);
                }
            }
            catch (System.Exception)
            {

            }
        }
    }
}