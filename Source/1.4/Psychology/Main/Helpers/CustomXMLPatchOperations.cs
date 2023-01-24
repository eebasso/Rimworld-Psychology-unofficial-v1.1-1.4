﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using RimWorld;
using Verse;
using Unity;
using UnityEngine;
using System.IO;
using System.Xml.Linq;

namespace Psychology;

public class PatchOperationAddOrCreate : PatchOperationPathed
{
  public XmlContainer value;

  protected override bool ApplyWorker(XmlDocument xml)
  {
    bool result = false;
    XmlNode valueNode = value.node;
    string xpathUpOneLevel = xpath.Substring(0, xpath.LastIndexOf('/'));
    string lastNodeLabel = xpath.Split('/').Last();
    ////Log.Message("xpath = " + xpath);
    ////Log.Message("xpathUpOneLevel = " + xpathUpOneLevel);
    ////Log.Message("lastNodeLabel = " + lastNodeLabel);
    if (xml.SelectSingleNode(xpath) != null)
    {
      ////Log.Message("xml.SelectSingleNode(xpath) != null");
      foreach (object item in xml.SelectNodes(xpath))
      {
        ////Log.Message("Select item in xml.SelectNodes(xpath)");
        result = true;
        XmlNode xmlNode = item as XmlNode;
        foreach (XmlNode childNode in valueNode.ChildNodes)
        {
          ////Log.Message("XmlNode childNode in valueNode.ChildNodes");
          xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
        }
      }
      return result;
    }
    ////Log.Message("xml.SelectSingleNode(xpath) == null");
    foreach (object item in xml.SelectNodes(xpathUpOneLevel))
    {
      ////Log.Message("Select item in xml.SelectNodes(xpathUpOneLevel)");
      XmlNode xmlNode = item as XmlNode;
      XmlNode xmlNode2 = xmlNode[lastNodeLabel];
      if (xmlNode2 == null)
      {
        ////Log.Message("xmlNode2 == null");
        xmlNode2 = xmlNode.OwnerDocument.CreateElement(lastNodeLabel);
        xmlNode.AppendChild(xmlNode2);
      }
      foreach (XmlNode childNode in valueNode.ChildNodes)
      {
        ////Log.Message("XmlNode childNode in valueNode.ChildNodes");
        xmlNode2.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
      }
      result = true;
    }
    return result;
  }
}

public class PatchOperationReplaceKinseyEnabled : PatchOperationReplace
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    if (PsychologySettings.enableKinsey != true)
    {
      return true;
    }
    return base.ApplyWorker(xml);
  }
}

public class PatchOperationTraitMultiplier : PatchOperationPathed
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    bool result = false;
    foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
    {
      if (xmlNode.Value != null && xmlNode.NodeType == XmlNodeType.Text)
      {
        result = true;
        int oldModifier = ParseHelper.FromString<int>(xmlNode.Value);
        int newModifier = Mathf.CeilToInt(PsychologySettings.traitOpinionMultiplier * oldModifier);
        xmlNode.Value = newModifier.ToString();
      }
    }
    return result;
  }
}

public class PatchOperationImprisonedMe : PatchOperationPathed
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    bool result = false;
    foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
    {
      ////Log.Message("Foreach xmlNode");
      if (xmlNode.Value != null && xmlNode.NodeType == XmlNodeType.Text)
      {
        ////Log.Message("xmlNode.Value != null: " + xmlNode.Value);
        result = true;
        xmlNode.Value = (-PsychologySettings.imprisonedDebuff).ToString();
        ////Log.Message("xmlNode.Value set to new modifier");
      }
    }
    if (result != true)
    {
      Log.Error("PatchOperationImprisonedMe");
    }

    return result;
  }
}

public class PatchOperationRapportBuilt : PatchOperationPathed
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    bool result = false;
    foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
    {
      if (xmlNode.Value != null && xmlNode.NodeType == XmlNodeType.Text)
      {
        ////Log.Message("xmlNode.Value != null: " + xmlNode.Value);
        result = true;
        float oldValue = ParseHelper.FromString<int>(xmlNode.Value);
        float newValue = Mathf.Lerp(oldValue, 0f, PsychologySettings.imprisonedDebuff / PsychologySettings.imprisonedDebuffDefault);
        xmlNode.Value = newValue.ToString();
        ////Log.Message("xmlNode.Value set to new modifier");
      }
    }
    return result;
  }
}

public class PatchOperationAddOrCreateEmpathyEnabled : PatchOperationAddOrCreate
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    if (!PsychologySettings.enableEmpathy)
    {
      return true;
    }
    return base.ApplyWorker(xml);
  }
}

public class PatchOperationReplaceEmpathyEnabled : PatchOperationReplace
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    if (!PsychologySettings.enableEmpathy)
    {
      return true;
    }
    return base.ApplyWorker(xml);
  }
}

public class PatchOperationAttributeAddEmpathyEnabled : PatchOperationAttributeAdd
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    if (!PsychologySettings.enableEmpathy != true)
    {
      return true;
    }
    return base.ApplyWorker(xml);
  }
}

public class PatchOperationConversationDurationDays : PatchOperationPathed
{
  protected override bool ApplyWorker(XmlDocument xml)
  {
    bool result = false;
    foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
    {
      if (xmlNode.Value != null && xmlNode.NodeType == XmlNodeType.Text)
      {
        result = true;
        xmlNode.Value = PsychologySettings.conversationDuration.ToString();
      }
    }
    if (result != true)
    {
      Log.Error("PatchOperationImprisonedMe");
    }

    return result;
  }
}