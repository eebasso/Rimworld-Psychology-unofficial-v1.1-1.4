using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class PersonalityNodeDef : Def
    {

        /* Being a woman has an 80% chance to modify this node by this amount, reduced by how gay she is.
         * This models the cultural impact traditional gender roles have on their personality. (Lesbians/bisexuals, obviously, tend to subvert them.)
         * Even in 55XX, the patriarchy has not been vanquished. /s
         */
        public float femaleModifier;
        //A list of the DefNames of the parents of this node.
        public List<PersonalityNodeParent> parents;
        //What pawns talk about when they talk about this node.
        public List<string> conversationTopics;
        //What pawns with a high rating in this node use as a platform issue.
        public string platformIssueHigh;
        //What pawns with a low rating in this node use as a platform issue.
        public string platformIssueLow;
        //A list of the skills that modify this node.
        public List<PersonalityNodeSkillModifier> skillModifiers;
        //A list of the traits that modify this node.
        public List<PersonalityNodeTraitModifier> traitModifiers;
        //A list of the work types that being incapable of modify this node.
        public List<PersonalityNodeIncapableModifier> incapableModifiers;
        //How much a difference (or similarity) in this node affects what pawns think of each other after a conversation.
        public float controversiality;
        //The hours of the day that people with a high rating in this node will prefer to go on dates.
        public List<int> preferredDateHours;
        //Antonym Name
        public string oppositeName;
        //Label for node in the description
        public string descriptionLabel;
        //Node Color
        public Vector3 nodeHSV;
        //Antonym Color
        public Vector3 oppositeHSV;

        //A list of the actual parent Defs of this node.
        [Unsaved]
        private Dictionary<PersonalityNodeDef, PersonalityNodeParent> parentDict;

        public void ReloadParents()
        {
            parentDict.Clear();
        }

        public float GetModifier(PersonalityNodeDef def)
        {
            float m = ParentNodes[def].modifier;
            return m == 0f ? 1f : m;
        }

        public Dictionary<PersonalityNodeDef, PersonalityNodeParent> ParentNodes
        {
            get
            {
                //Log.Message("Inside PersonalityNodeDef.ParentNodes for " + this.label);
                if (this.parentDict == null)
                {
                    //Log.Message("this.parendDict was null");
                    this.parentDict = new Dictionary<PersonalityNodeDef, PersonalityNodeParent>();
                    //Log.Message("Initialized this.parentDict");
                    if (this.parents != null && this.parents.Count > 0)
                    {
                        //Log.Message("Inside if (this.parents != null && this.parents.Count > 0)");
                        foreach (PersonalityNodeParent parent in this.parents)
                        {
                            //Log.Message("Adding " + parent.node.label + " to " + this.label);
                            this.parentDict.Add(parent.node, parent);
                            //Log.Message("Added " + parent.node.label + " to " + this.label);
                        }
                    }
                }
                //Log.Message("Finished PersonalityNodeDef.ParentNodes for " + this.label);
                //Log.Message("this.parentDict contains:");
                //foreach (KeyValuePair<PersonalityNodeDef, PersonalityNodeParent> thing in this.parentDict)
                //{
                //    Log.Message("{" + thing.Key.label + ", " + thing.Value.node.label + "}");
                //}
                return this.parentDict;
            }
        }

        public override int GetHashCode()
        {
            return this.defName.GetHashCode();
        }



    }
}
