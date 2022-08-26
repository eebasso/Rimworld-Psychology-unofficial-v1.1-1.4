using System;
using System.Linq;
using System.Xml;
using HarmonyLib;
using Verse;
using Unity;
using UnityEngine;
using HugsLib;

namespace Psychology
{
    public class PatchOperationCustom : PatchOperationPathed
    {
        public override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Message("Psychology.PatchOperationCustom");
            bool result = false;
            foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
            {
                //Log.Message("Foreach xmlNode");
                if (xmlNode.Value != null && xmlNode.NodeType == XmlNodeType.Text)
                {
                    //Log.Message("xmlNode.Value != null: " + xmlNode.Value);
                    result = true;
                    int oldModifier = ParseHelper.FromString<int>(xmlNode.Value);
                    //Log.Message("Old modifier was " + oldModifier);
                    int newModifier = Mathf.CeilToInt(PsychologySettings.traitOpinionMultiplier * oldModifier);
                    xmlNode.Value = newModifier.ToString();
                    //Log.Message("xmlNode.Value set to new modifier");
                }
            }
            return result;
        }
    }

    public class EnableEmpathyPatchOperationAdd : PatchOperationAdd
    {
        public override bool ApplyWorker(XmlDocument xml)
        {
            if (!PsychologySettings.enableElections)
            {
                return false;
            }
            return base.ApplyWorker(xml);

        }
    }

    public class EnableEmpathyPatchOperationAttirbuteAdd : PatchOperationAttributeAdd
    {
        public override bool ApplyWorker(XmlDocument xml)
        {
            if (!PsychologySettings.enableElections)
            {
                return false;
            }
            return base.ApplyWorker(xml);
        }
    }


}

