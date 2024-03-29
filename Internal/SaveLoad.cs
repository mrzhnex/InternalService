﻿using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace InternalService.Internal
{
    public static class SaveLoad
    {
        public static readonly XmlSerializer ReportersFormatter = new XmlSerializer(typeof(List<ulong>));
        public static readonly XmlSerializer ReportIdFormatter = new XmlSerializer(typeof(ulong));

        public static void LoadReportId()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Info.HostDataPath, Info.ReportIdFileName), FileMode.Open))
                {
                    Manage.ReportId = (ulong)ReportIdFormatter.Deserialize(fs);
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
                using (FileStream fs = new FileStream(Path.Combine(Info.HostDataPath, Info.ReportIdFileName), FileMode.Open))
                {
                    ReportIdFormatter.Serialize(fs, Manage.ReportId);
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
                using (FileStream fs = new FileStream(Path.Combine(Info.HostDataPath, Info.ReportersFileName), FileMode.Open))
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
                using (FileStream fs = new FileStream(Path.Combine(Info.HostDataPath, Info.ReportersFileName), FileMode.Open))
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