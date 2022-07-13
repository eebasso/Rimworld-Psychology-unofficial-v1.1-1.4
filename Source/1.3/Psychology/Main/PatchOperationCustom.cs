//using System;
//using System.Xml;
//using HarmonyLib;
//using Verse;

//namespace Psychology
//{
//    public class PatchOperationCustom : PatchOperationPathed
//    {
//        protected override bool ApplyWorker(XmlDocument xml)
//        {
//            bool matched = false;
//            foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
//            {
//                matched = true;
//                //do something here
//            }
//            return matched;
//        }
//    }
//}

